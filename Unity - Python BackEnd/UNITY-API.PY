from flask import Flask, jsonify, request
import numpy as np
from pylsl import StreamInlet, resolve_byprop
import utils  # Utility functions (ensure utils.py is available)
import threading
import time
import os

os.environ["FLASK_SKIP_DOTENV"] = "1"
app = Flask(__name__)
class Band:
    Delta = 0
    Theta = 1
    Alpha = 2
    Beta = 3

BUFFER_LENGTH = 3
EPOCH_LENGTH = 1  
OVERLAP_LENGTH = 0.8
SHIFT_LENGTH = EPOCH_LENGTH - OVERLAP_LENGTH 
INDEX_CHANNEL = [0]
streaming = False
metrics = {"eeg": {}, "gyro_acc": {}, "ppg": {}}
lock = threading.Lock()


def process_ppg(ppg_data, sampling_rate):
    ppg_signal = np.array(ppg_data).flatten()
    peaks = utils.detect_peaks(ppg_signal, distance=int(sampling_rate * 0.6)) 
    ibi = np.diff(peaks) / sampling_rate  
    
    if len(ibi) > 1:
        heart_rate = 60 / np.mean(ibi)  
        hrv_sdnn = np.std(ibi)  
    else:
        heart_rate = None
        hrv_sdnn = None

    return {"Heart_Rate": heart_rate, "HRV_SDNN": hrv_sdnn}


def start_streaming():
    global streaming, metrics

    print("Looking for streams...")
    eeg_streams = resolve_byprop("type", "EEG", timeout=2)
    acc_streams = resolve_byprop("type", "ACC", timeout=2)
    gyro_streams = resolve_byprop("type", "GYRO", timeout=2)
    ppg_streams = resolve_byprop("type", "PPG", timeout=2)

    if len(eeg_streams) == 0:
        raise RuntimeError("Can't find EEG stream.")
    if len(acc_streams) == 0:
        raise RuntimeError("Can't find ACC stream.")
    if len(gyro_streams) == 0:
        raise RuntimeError("Can't find GYRO stream.")
    if len(ppg_streams) == 0:
        raise RuntimeError("Can't find PPG stream.")

    eeg_inlet = StreamInlet(eeg_streams[0], max_chunklen=12)
    acc_inlet = StreamInlet(acc_streams[0], max_chunklen=12)
    gyro_inlet = StreamInlet(gyro_streams[0], max_chunklen=12)
    ppg_inlet = StreamInlet(ppg_streams[0], max_chunklen=12)

    fs_eeg = int(eeg_inlet.info().nominal_srate())
    fs_ppg = int(ppg_inlet.info().nominal_srate())

    eeg_buffer = np.zeros((int(fs_eeg * BUFFER_LENGTH), 1))
    filter_state = None
    n_win_test = int(np.floor((BUFFER_LENGTH - EPOCH_LENGTH) / SHIFT_LENGTH + 1))
    band_buffer = np.zeros((n_win_test, 4))

    print("Streaming started. Press Ctrl-C to stop.")

    try:
        while streaming:
            """Process EEG Data"""
            eeg_data, _ = eeg_inlet.pull_chunk(timeout=1, max_samples=int(SHIFT_LENGTH * fs_eeg))
            acc_data, _ = acc_inlet.pull_chunk(timeout=1)
            gyro_data, _ = gyro_inlet.pull_chunk(timeout=1)
            ppg_data, _ = ppg_inlet.pull_chunk(timeout=1)

            if eeg_data:
                ch_data = np.array(eeg_data)[:, INDEX_CHANNEL]
                eeg_buffer, filter_state = utils.update_buffer(
                    eeg_buffer, ch_data, notch=True, filter_state=filter_state
                )

                data_epoch = utils.get_last_data(eeg_buffer, EPOCH_LENGTH * fs_eeg)
                band_powers = utils.compute_band_powers(data_epoch, fs_eeg)
                band_buffer, _ = utils.update_buffer(
                    band_buffer, np.asarray([band_powers])
                )
                smooth_band_powers = np.mean(band_buffer, axis=0)

                alpha_metric = (
                    smooth_band_powers[Band.Alpha] / smooth_band_powers[Band.Delta]
                )
                beta_theta_metric = (
                    smooth_band_powers[Band.Beta] / smooth_band_powers[Band.Theta]
                )

                with lock:
                    metrics["eeg"] = {
                        "Delta": smooth_band_powers[Band.Delta],
                        "Theta": smooth_band_powers[Band.Theta],
                        "Alpha": smooth_band_powers[Band.Alpha],
                        "Beta": smooth_band_powers[Band.Beta],
                    }
                    metrics["relaxation"] = {
                        "Alpha_Relaxation": alpha_metric,
                        "Beta_Theta_Concentration": beta_theta_metric,
                    }

            if acc_data and gyro_data:
                with lock:
                    metrics["gyro_acc"] = {
                        "acc": np.mean(acc_data, axis=0).tolist(),
                        "gyro": np.mean(gyro_data, axis=0).tolist(),
                    }

            if ppg_data:
                ppg_metrics = process_ppg(ppg_data, fs_ppg)
                with lock:
                    metrics["ppg"] = ppg_metrics

            time.sleep(SHIFT_LENGTH)

    except KeyboardInterrupt:
        print("Streaming stopped.")
    finally:
        streaming = False


@app.route("/start", methods=["POST"])
def start_stream():
    global streaming
    if streaming:
        return jsonify({"message": "Stream already running."}), 400
    streaming = True
    threading.Thread(target=start_streaming).start()
    return jsonify({"message": "Streaming started."})


@app.route("/stop", methods=["POST"])
def stop_stream():
    global streaming
    streaming = False
    return jsonify({"message": "Streaming stopped."})


@app.route("/metrics", methods=["GET"])
def get_metrics():
    with lock:
        if not any(metrics.values()):
            return jsonify({"message": "No data available yet. Please wait for streaming."}), 202
        return jsonify(metrics)

@app.route("/calmness_score", methods=["GET"])
def calculate_calmness_score(alpha_relaxation, beta_theta_concentration):
    max_alpha = 2  
    max_beta_theta = 2 

    alpha_score = min(max(alpha_relaxation / max_alpha, 0), 1)  
    beta_score = 1 - min(max(beta_theta_concentration / max_beta_theta, 0), 1)  
    print(f"Normalized Alpha Score: {alpha_score}, Beta Score: {beta_score}")

    alpha_weight = 0.7
    beta_weight = 0.3
    calmness = alpha_weight * alpha_score + beta_weight * beta_score

    calmness_score = int(round(calmness * 4 + 1))

    print(f"Calmness Score: {calmness_score}")
    


        
    return jsonify({"calmness_score": calmness_score})

def calculate_calmness_score(alpha_relaxation, beta_theta_concentration):
    alpha_weight = 0.7  
    beta_weight = 0.3   

    alpha_score = min(max(alpha_relaxation / 5, 0), 1)  
    beta_score = 1 - min(max(beta_theta_concentration / 5, 0), 1)
    calmness = alpha_weight * alpha_score + beta_weight * beta_score
    calmness_score = int(round(calmness * 4 + 1))
    return calmness_score
def calculate_stress_level(heart_rate, hrv_sdnn, beta_theta_concentration):
    def map_heart_rate_to_stress(hr):
        if hr < 60:
            return 1  # Very relaxed
        elif 60 <= hr < 70:
            return 2  # Relaxed
        elif 70 <= hr < 85:
            return 3  # Moderate
        elif 85 <= hr < 100:
            return 4  # High stress
        else:
            return 5  # Very high stress
    def hrv_stress_modifier(sdnn):
        if sdnn > 0.5:
            return -1  # Reduces stress level
        elif 0.2 <= sdnn <= 0.5:
            return 0   # Neutral
        else:
            return 1   # Increases stress level
    def brain_wave_stress_modifier(beta_theta):
        if beta_theta < 0.1:
            return -1  # Very relaxed
        elif 0.1 <= beta_theta < 0.3:
            return 0   # Neutral
        else:
            return 1   # Increased stress
    stress_level = map_heart_rate_to_stress(heart_rate)
    stress_level += hrv_stress_modifier(hrv_sdnn)
    stress_level += brain_wave_stress_modifier(beta_theta_concentration)
    return max(1, min(5, stress_level))

def calculate_stress_level(heart_rate):
    def heart_rate_to_stress(hr):

        if hr < 75:
            return 1  # Extremely relaxed (potential athletic conditioning)
        elif 75 <= hr < 85:
            return 2  # Relaxed state
        elif 85 <= hr < 90:
            return 3  # Neutral/baseline stress
        elif 90 <= hr < 95:
            return 4  # Elevated stress
        else:
            return 5  # High stress/potential health concern

    stress_level = heart_rate_to_stress(heart_rate)
    
    return stress_level

@app.route('/vr_score', methods=['GET'])
def process_vr_stress():
    with lock:
        if not metrics.get('ppg') or not metrics['ppg'].get('Heart_Rate'):
            return '0', 400 

        heart_rate = metrics['ppg']['Heart_Rate']
        stress_level = calculate_stress_level(heart_rate)
        return str(stress_level), 200
if __name__ == "__main__":
    app.run(debug=True, use_reloader=False)

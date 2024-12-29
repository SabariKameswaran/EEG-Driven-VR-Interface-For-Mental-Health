from flask import Flask, Response
import numpy as np
import threading
import time
import os

os.environ["FLASK_SKIP_DOTENV"] = "1"

# Flask app initialization
app = Flask(__name__)

# Constants for EEG processing
BUFFER_LENGTH = 5  # Seconds
EPOCH_LENGTH = 1  # Seconds
OVERLAP_LENGTH = 0.8  # Seconds
SHIFT_LENGTH = EPOCH_LENGTH - OVERLAP_LENGTH  # Shift length in seconds
INDEX_CHANNEL = [0]  # Index of the EEG channel to process

# Global variables
streaming = False
metrics = {"eeg": {}, "gyro_acc": {}, "ppg": {}}
lock = threading.Lock()

# Helper functions for scoring
def normalize(value, min_value, max_value):
    """Normalize a value between 0 and 1."""
    return (value - min_value) / (max_value - min_value)

def calculate_vr_score(alpha_relaxation, beta_theta_concentration, heart_rate):
    """
    Calculate a unified VR score based on EEG, heart rate, and accelerometer data.
    """
    # Normalize EEG metrics
    alpha_score = normalize(alpha_relaxation, 0, 2)  # Scale Alpha Relaxation
    beta_theta_score = normalize(beta_theta_concentration, 0, 2)  # Scale Beta/Theta Ratio

    # Map heart rate to stress score
    if heart_rate < 60:
        heart_rate_score = 0  # Very calm
    elif 60 <= heart_rate < 80:
        heart_rate_score = 1  # Neutral
    elif 80 <= heart_rate < 100:
        heart_rate_score = 2  # Slightly stressed
    else:
        heart_rate_score = 3  # Highly stressed

    # Weighted average to compute VR score
    alpha_weight = 0.5
    beta_theta_weight = 0.3
    heart_rate_weight = 0.2

    calmness = alpha_weight * (1 - alpha_score) + beta_theta_weight * (1 - beta_theta_score)
    stress = heart_rate_weight * heart_rate_score

    # Final VR score (0 to 5 scale)
    vr_score = calmness * 5 - stress
    return max(0, min(5, vr_score))  # Ensure score is between 0-5

def process_vr_data():
    """Fetch metrics and compute the VR score."""
    with lock:
        if not metrics["eeg"] or not metrics["ppg"] or not metrics["gyro_acc"]:
            return "Error: Insufficient data to compute VR score", 400

        # Extract EEG relaxation metrics
        alpha_relaxation = metrics["eeg"]["Alpha_Relaxation"]
        beta_theta_concentration = metrics["eeg"]["Beta_Theta_Concentration"]

        # Extract heart rate
        heart_rate = metrics["ppg"].get("Heart_Rate", 70)  # Default to 70 BPM if unavailable

        # Compute VR score
        vr_score = calculate_vr_score(alpha_relaxation, beta_theta_concentration, heart_rate)

        # Convert the float VR score to an integer
        vr_score_int = round(vr_score)  # Round the float to the nearest integer
        
        return str(vr_score_int), 200  # Return the integer VR score as a string

def start_streaming():
    """Simulate data streaming for EEG, PPG, and gyro/accelerometer data."""
    global streaming, metrics

    print("Data streaming started...")

    while streaming:
        with lock:
            metrics["eeg"] = {
                "Alpha_Relaxation": np.random.uniform(0.5, 1.5),
                "Beta_Theta_Concentration": np.random.uniform(0.5, 1.5),
            }

            metrics["ppg"] = {
                "Heart_Rate": np.random.randint(60, 100),
            }

            metrics["gyro_acc"] = {
                "Movement": np.random.uniform(0.1, 1.0),
            }

        time.sleep(1)

    print("Data streaming stopped.")

# Flask endpoints
@app.route("/start", methods=["POST"])
def start_stream():
    """Start the data stream."""
    global streaming
    if streaming:
        return Response("Stream already running.", status=400)
    streaming = True
    threading.Thread(target=start_streaming).start()
    return Response("Streaming started.", status=200)

@app.route("/stop", methods=["POST"])
def stop_stream():
    """Stop the data stream."""
    global streaming
    streaming = False
    return Response("Streaming stopped.", status=200)

@app.route("/metrics", methods=["GET"])
def get_metrics():
    """Fetch the latest metrics."""
    with lock:
        if not any(metrics.values()):
            return Response("No data available yet. Please wait for streaming.", status=202)
        return Response(str(metrics), status=200)

@app.route("/vr_score", methods=["GET"])
def get_vr_score():
    """Compute and return the VR score as an integer."""
    vr_data, status_code = process_vr_data()
    return Response(vr_data, status=status_code)  # Return the integer VR score as raw data

if __name__ == "__main__":
    app.run(debug=True, use_reloader=False)

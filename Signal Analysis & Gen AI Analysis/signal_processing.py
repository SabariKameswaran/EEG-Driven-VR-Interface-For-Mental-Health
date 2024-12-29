import streamlit as st
import numpy as np
import pandas as pd
from scipy.fft import fft
import matplotlib.pyplot as plt

# Set up Streamlit
st.title("EEG Band Analysis for All Channels")
st.markdown("""
This app analyzes EEG signals across standard frequency bands:
- **Delta**: 0.5–4 Hz
- **Theta**: 4–8 Hz
- **Alpha**: 8–13 Hz
- **Beta**: 13–30 Hz
- **Gamma**: 30–50 Hz
""")

# File upload
uploaded_file = st.file_uploader("Upload your EEG CSV file", type=["csv"])

if uploaded_file is not None:
    # Load the data
    eeg_data = pd.read_csv(uploaded_file)
    st.write("Data Preview:", eeg_data.head())

    # Define sampling frequency
    fs = st.number_input("Sampling Frequency (Hz)", min_value=1, max_value=1000, value=256, step=1)

    # Define EEG band ranges
    eeg_bands = {
        'Delta': (0.5, 4),
        'Theta': (4, 8),
        'Alpha': (8, 13),
        'Beta': (13, 30),
        'Gamma': (30, 50),
    }

    # Choose a channel for detailed analysis
    eeg_channels = eeg_data.columns[1:]  # Skip 'timestamps'
    selected_channel = st.selectbox("Select EEG Channel", eeg_channels)

    # Extract the selected channel
    eeg_signal = eeg_data[selected_channel].values

    # Compute FFT
    n = len(eeg_signal)
    freqs = np.fft.fftfreq(n, d=1/fs)
    positive_freq_indices = freqs >= 0
    positive_freqs = freqs[positive_freq_indices]
    dft = fft(eeg_signal)
    dft_amplitude = np.abs(dft[positive_freq_indices])

    # Calculate total power
    total_power = np.sum(dft_amplitude**2)

    # Calculate band powers
    band_powers = {}
    for band, (low, high) in eeg_bands.items():
        band_indices = np.logical_and(positive_freqs >= low, positive_freqs <= high)
        band_power = np.sum(dft_amplitude[band_indices]**2)
        band_powers[band] = {
            'Power': band_power,
            'Contribution (%)': (band_power / total_power) * 100,
        }

    # Display band powers
    st.markdown(f"### EEG Band Powers for {selected_channel}")
    for band, stats in band_powers.items():
        st.write(f"**{band}**: Power = {stats['Power']:.2f}, Contribution = {stats['Contribution (%)']:.2f}%")

    # Plot power spectrum
    st.markdown(f"### Power Spectrum for {selected_channel}")
    fig, ax = plt.subplots(figsize=(10, 6))
    ax.plot(positive_freqs, dft_amplitude, color='blue')
    ax.set_title(f"Frequency Spectrum ({selected_channel})")
    ax.set_xlabel("Frequency (Hz)")
    ax.set_ylabel("Amplitude")
    ax.grid(True)
    st.pyplot(fig)

    # Plot power spectra for each band
    st.markdown(f"### Band Power Spectra for {selected_channel}")
    for band, (low, high) in eeg_bands.items():
        band_indices = np.logical_and(positive_freqs >= low, positive_freqs <= high)
        band_freqs = positive_freqs[band_indices]
        band_power = dft_amplitude[band_indices]

        fig, ax = plt.subplots(figsize=(8, 4))
        ax.plot(band_freqs, band_power, label=f"{band} Band ({low}-{high} Hz)", color='green')
        ax.set_title(f"{selected_channel} - {band} Band Power Spectrum")
        ax.set_xlabel("Frequency (Hz)")
        ax.set_ylabel("Amplitude")
        ax.legend()
        ax.grid(True)
        st.pyplot(fig)

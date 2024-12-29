import streamlit as st
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from fpdf import FPDF
import io
from datetime import datetime
from requests.exceptions import ConnectionError

try:
    from groq import Groq
except ImportError:
    st.warning("Groq library is not installed. Install it using `pip install groq`.")


# Helper Functions
def initialize_groq_client(api_key):
    """Initialize Groq client with the provided API key."""
    try:
        client = Groq(api_key=api_key)
        return client
    except Exception as e:
        st.error(f"Error initializing Groq client: {e}")
        return None


def generate_ai_insights(data, api_key):
    """Generate AI-powered clinical insights using Groq."""
    if not api_key:
        st.warning("Please enter a valid Groq API key to enable AI insights.")
        return "AI insights unavailable"

    client = initialize_groq_client(api_key)
    if not client:
        return "AI insights unavailable"

    # Prepare data summary for AI analysis
    summary_stats = data.describe().T
    correlation_matrix = data.corr()

    # Construct prompt for AI analysis
    prompt = f"""Provide a professional clinical analysis of the following brainwave data:

Data Summary:
{summary_stats}

Correlation Matrix:
{correlation_matrix}

Please generate:
1. Key observations about brainwave patterns
2. Potential clinical implications
3. Recommendations for further investigation
4. Any anomalies or unusual patterns detected"""

    try:
        chat_completion = client.chat.completions.create(
            messages=[
                {"role": "system", "content": "You are a clinical neuroscience expert analyzing brainwave data."},
                {"role": "user", "content": prompt},
            ],
            model="mixtral-8x7b-32768",
        )
        return chat_completion.choices[0].message.content
    except ConnectionError:
        st.error("Connection error occurred. Please check your network and API key.")
        return "AI analysis failed due to connection issues."
    except Exception as e:
        st.error(f"Error generating AI insights: {e}")
        return "AI analysis failed due to an unknown error."


def generate_downloadable_report(data, ai_insights):
    """Generate a PDF report with AI insights."""
    pdf = FPDF()
    pdf.add_page()
    pdf.set_font("Arial", size=12)

    # Title
    pdf.cell(200, 10, txt="Brainwave Data Clinical Report", ln=True, align='C')

    # Data Summary
    pdf.cell(200, 10, txt="Data Summary Statistics", ln=True)
    summary_stats = data.describe().T
    for index, row in summary_stats.iterrows():
        pdf.multi_cell(0, 10, f"{index}: Mean={row['mean']:.2f}, Std={row['std']:.2f}")

    # AI Insights
    pdf.add_page()
    pdf.cell(200, 10, txt="AI-Generated Clinical Insights", ln=True)
    pdf.multi_cell(0, 10, ai_insights)

    # Output as bytes
    return pdf.output(dest="S").encode("latin1")


def download_ai_insights(ai_insights):
    """Provide a downloadable button for AI insights as a text file."""
    if ai_insights:
        insights_buffer = io.StringIO()
        insights_buffer.write(ai_insights)
        insights_buffer.seek(0)

        st.download_button(
            label="Download AI Insights as Text File",
            data=insights_buffer.getvalue(),
            file_name=f"ai_insights_{datetime.now().strftime('%Y%m%d_%H%M%S')}.txt",
            mime="text/plain",
        )


# Main Function
def main():
    st.title("Advanced Clinical Brainwave Analysis Dashboard")
    st.sidebar.header("API Configuration")
    groq_api_key = st.sidebar.text_input("Enter Groq API Key", type="password")

    st.write("Comprehensive tool for neurological data exploration and insights.")

    # Upload CSV file
    data_file = st.file_uploader("Upload your brainwave data CSV file", type=["csv"])

    if data_file is not None:
        # Load data
        data = pd.read_csv(data_file)

        # Detect Unix timestamp columns
        st.write("### Data Overview")
        st.dataframe(data.head())

        # Preprocess Unix timestamps
        st.sidebar.header("Preprocessing Options")
        time_column = st.sidebar.selectbox(
            "Select the time column (Unix format)", [col for col in data.columns if 'time' in col.lower()]
        )

        if time_column:
            try:
                # Convert Unix time to human-readable datetime
                data[time_column] = pd.to_datetime(data[time_column], unit='s')  # Change 's' to 'ms' if in milliseconds
                st.success(f"Converted {time_column} to datetime format.")
            except Exception as e:
                st.error(f"Error converting {time_column}: {e}")

        st.write("### Processed Data Overview")
        st.dataframe(data.head())

        # Visualization Options
        plot_options = [
            "Brainwave Trends (Line Plot)",
            "Distributions and Outliers (Box Plot)",
            "Brainwave Distribution (Histogram)",
            "Correlation Analysis (Heatmap)",
        ]
        selected_plot = st.sidebar.selectbox("Choose a plot type", plot_options)

        # Generate selected plot
        if selected_plot == "Brainwave Trends (Line Plot)":
            st.write("### Brainwave Trends Over Time")
            metric_column = st.sidebar.selectbox("Select the brainwave metric", [col for col in data.columns if col != time_column])
            fig, ax = plt.subplots()
            sns.lineplot(x=data[time_column], y=data[metric_column], ax=ax)
            ax.set_title(f"Trend of {metric_column} Over Time")
            st.pyplot(fig)

        elif selected_plot == "Distributions and Outliers (Box Plot)":
            st.write("### Box Plot for Brainwave Metrics")
            column = st.sidebar.selectbox("Select a metric", data.columns)
            fig, ax = plt.subplots()
            sns.boxplot(x=data[column], ax=ax)
            ax.set_title(f"Box Plot for {column}")
            st.pyplot(fig)

        elif selected_plot == "Brainwave Distribution (Histogram)":
            st.write("### Distribution of Brainwave Metrics")
            column = st.sidebar.selectbox("Select a metric", data.columns)
            bins = st.sidebar.slider("Number of bins", 5, 100, 20)
            fig, ax = plt.subplots()
            sns.histplot(data[column], bins=bins, kde=True, ax=ax)
            ax.set_title(f"Distribution of {column}")
            st.pyplot(fig)

        elif selected_plot == "Correlation Analysis (Heatmap)":
            st.write("### Correlation Heatmap")
            fig, ax = plt.subplots(figsize=(10, 8))
            correlation_matrix = data.corr()
            sns.heatmap(correlation_matrix, annot=True, cmap="coolwarm", ax=ax)
            ax.set_title("Correlation Between Metrics")
            st.pyplot(fig)

        # AI Insights using Groq
        if st.button("Generate AI Insights"):
            with st.spinner("Querying Groq for insights..."):
                ai_insights = generate_ai_insights(data, api_key=groq_api_key)
                st.write("### AI-Generated Clinical Insights")
                st.write(ai_insights)

                # Download AI insights as text
                download_ai_insights(ai_insights)

                # Generate and download PDF report
                report_buffer = generate_downloadable_report(data, ai_insights)
                st.download_button(
                    label="Download PDF Report",
                    data=report_buffer,
                    file_name="clinical_report.pdf",
                    mime="application/pdf",
                )


if __name__ == "__main__":
    main()

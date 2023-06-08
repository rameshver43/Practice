from flask import Flask, render_template, request
import traceback
import speech_recognition as sr

app = Flask(__name__)

@app.route('/')
def index():
    return render_template('index.html')

@app.route('/upload', methods=['POST'])
def upload():
    try:
        audio_file = request.files['audio']
        transcription = transcribe_audio(audio_file)
        return transcription
    except Exception as e:
        traceback.print_exc()
        return "Error: " + str(e), 500

def transcribe_audio(audio_file):
    recognizer = sr.Recognizer()

    with sr.AudioFile(audio_file) as source:
        audio_data = recognizer.record(source)

    try:
        text = recognizer.recognize_google(audio_data)
        return text
    except sr.UnknownValueError:
        return "Speech not recognized"
    except sr.RequestError as e:
        return f"Error: {e}"

if __name__ == '__main__':
    app.run()

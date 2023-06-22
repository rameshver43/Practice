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






Dear All
w.r.t to user query request classification and what should exactly be included in the program schedule list for the demo tomorrow

Here it is

Just a small tweak to what we discussed on how the request has to be classified.

User Request: includes keywords Affirmations, Gratitude --> Category (video)
e.g. https://www.youtube.com/watch?v=yo1pJ_D-H3M


User Request: includes meditation with sadhguru --> Category (video)
e.g. https://www.youtube.com/watch?v=PsKEgrx0Xwc

User Request: includes yoga with sadhguru --> category (video)

e.g. https://www.youtube.com/watch?v=EVb7mE9VtW8

User Request: includes Biography   --> Category wiki with image (nltk request)
e.g. https://en.wikipedia.org/wiki/Mahatma_Gandhi
output summarized biography with Q&A capability (nltk)

User Request: includes News --> Category (News)
e.g. https://www.wionews.com/videos
open this website in browser view


Make the search query refined internally to get one link (this exact link) for each of the user request as above

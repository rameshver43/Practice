from flask import Flask, render_template, request,jsonify
import traceback
from googleapiclient.discovery import build

app = Flask(__name__)
api_key = ''  #Rameshver Key
youtube = build('youtube', 'v3', developerKey=api_key)


def generate_video_info(input_text):
    search_response = youtube.search().list(
        q=input_text,
        part='id,snippet',
        maxResults=1,
        type='video'
    ).execute()

    video_info = {}
    if 'items' in search_response:
        item = search_response['items'][0]
        video_id = item['id']['videoId']
        video_title = item['snippet']['title']
        video_thumbnail = item['snippet']['thumbnails']['default']['url']
        youtube_link = f'https://www.youtube.com/watch?v={video_id}'

        video_info = {
            'title': video_title,
            'thumbnail': video_thumbnail,
            'link': youtube_link
        }

    return video_info

@app.route('/getyoutubedetails', methods=['GET'])
def getyoutubedetails():
    input_text = 'Play song from bollywood movie adipurush'
    video_info = generate_video_info(input_text)
    if video_info:
        result = {
            "Status":"Pass",
            "Video_Info":video_info
        }
        return jsonify(result)
    else:
        result = {
            "Status":"Fail",
            "Video_Info":'No video found for the given input text.'
        }
        return jsonify(result)

if __name__ == '__main__':
    app.run()
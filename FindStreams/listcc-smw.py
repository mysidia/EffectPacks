#!/usr/bin/python
import requests
import json
import csv
import os
import time

import apptoken
readTokens = apptoken.get_tokens()
client_id = readTokens[0]
client_secret = readTokens[1]
app_token = readTokens[2]
gameid = 'game_id=1505160567&game_id=1229'


#url1 = 'https://api.twitch.tv/helix/streams?first=100&'+str(gameid)
#print("Lookup request:" + url1)
#print("Search for channels with " + str(gameid) + " : " )
#lookup_request = requests.get(url1, headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token,
#         'Accept' : 'application/vnd.twitchtv.v5+json'})

#channel_name = input('Enter the Twitch channel I:')  
#lookup_request = requests.get('https://api.twitch.tv/helix/users?id=' + channel_name, headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token})


lookup_request = requests.get('https://api.twitch.tv/helix/extensions/live?'+
       'extension_id=7nydnrue11053qmjc6g0fd6einj75p',
       headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token})


if lookup_request.status_code == 200:
    j1 = json.loads(lookup_request.text)
    try:
        print(json.dumps(j1))
    except Exception as err:
        raise Exception("Could not lookup " + str(err))
    #print("SMW-Category:Viewers total=%s  streams=%s  median=%s" % (str(vtotal), str(scount), str(vmedian / 1) ))
else:
    raise Exception("Could not lookup " + str(lookup_request.text))	
    

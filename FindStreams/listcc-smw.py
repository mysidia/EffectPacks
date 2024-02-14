#!/usr/bin/python
import requests
import json
import csv
import os
import re
import time
import urllib.parse

import apptoken
readTokens = apptoken.get_tokens()
client_id = readTokens[0]
client_secret = readTokens[1]
app_token = readTokens[2]
gameid = 'game_id=1505160567&game_id=1229'

outjson = 1
if 'outjson' in os.environ and re.match(r'^\d+$', os.environ['outjson']):
    outjson = int( os.environ['outjson'] )

gamefilter_query = ''
if 'gamefilter' in os.environ:
    if re.search(r'&', os.environ['gamefilter']):
        gamefilter_query = '&'.join( 
            map(
                 lambda g: '='.join(map(
                    lambda h: urllib.parse.quote(h),
                    g.split('='))),
                 os.environ['gamefilter'].split('&')
            )
        )
    elif os.environ['gamefilter'] == '1':
        gamefilter_query = gameid
    elif re.match(r'^\d+$', os.environ['gamefilter']):
        gamefilter_query = f'game_id={ os.environ["gamefilter"] }'

#url1 = 'https://api.twitch.tv/helix/streams?first=100&'+str(gameid)
#print("Lookup request:" + url1)
#print("Search for channels with " + str(gameid) + " : " )
#lookup_request = requests.get(url1, headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token,
#         'Accept' : 'application/vnd.twitchtv.v5+json'})

#channel_name = input('Enter the Twitch channel I:')  
#lookup_request = requests.get('https://api.twitch.tv/helix/users?id=' + channel_name, headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token})

maximum_pagecount = 3
current_page = 0
first_page = True
pagination_string = None

while first_page or (pagination_string and len(pagination_string) > 0):
    first_page = False
    current_page = current_page + 1
    if current_page > maximum_pagecount:
         print(f'[Display limited to {maximum_pagecount} pages, {maximum_pagecount*100} entries]')
         break
    if pagination_string:
        pagination_query = f'&after={urllib.parse.quote(pagination_string)}'
    else:
        pagination_query = ''

    lookup_url = ('https://api.twitch.tv/helix/extensions/live?'+
          'extension_id=7nydnrue11053qmjc6g0fd6einj75p' + gamefilter_query + pagination_query)

    print(f"requests.get(\"{lookup_url}\")")
    lookup_request = requests.get(lookup_url,
           headers = {'Client-ID' : client_id,  'Authorization' : 'Bearer ' +app_token})
    pagination_string = None

    if lookup_request.status_code == 200:
        j1 = json.loads(lookup_request.text)
        try:
            for item in j1["data"]:
                if 'broadcaster_id' in item and re.match(r'^\d+$', item["broadcaster_id"]  ):
                    item["cclink"] = f'https://interact.crowdcontrol.live/#/twitch/{item["broadcaster_id"]}'
                    #time.sleep(1)
                    # ci = CrowdInteract()
                    # user_ccuid =  ci.getUserProfile_TwithID(  item["broadcaster_id"]  )
                    # session = ci.getUsersActiveGameSession(user_ccuid) # Query: /gameSession.getUsersActiveGameSession ccUID=x
                    # IF  session is Not null, then  print >
                    # time.sleep(1)
                #
            #
            if outjson:
                print(json.dumps(j1, indent=3))
            else:
                for item in j1["data"]:
                    print('%-20s  - %s %s' % ( item["broadcaster_name"], item["game_id"], item["game_name"]))
                    print('      - Title: %s' % (item["title"]))
                    print('      - Interact: %s' % (item["cclink"]))
            pagination_string = j1["pagination"]
        except Exception as err:
            raise Exception("Could not lookup " + str(err))
        #print("SMW-Category:Viewers total=%s  streams=%s  median=%s" % (str(vtotal), str(scount), str(vmedian / 1) ))
    else:
        raise Exception("Could not lookup " + str(lookup_request.text))	
    time.sleep(1)
#

 















   

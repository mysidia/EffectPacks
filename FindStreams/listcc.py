#!/usr/bin/python
#
#
# Search for streams with given Twitch extension active
#
#
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

# Twitch extension ID to search for
extension_query = f'extension_id=7nydnrue11053qmjc6g0fd6einj75p'

#    lookup_url = ('https://api.twitch.tv/helix/extensions/live?'+
#          extension_query + pagination_query)
#####


### Environment variables you can set to influence query results: ###
#
#       Set outjson to 0  to output as human readable
#       Set outjson to 1  to output results as JSON
#
# Example command line:    outjson=1  python3 listcc.py
#
outjson = 1
if ('outjson' in os.environ and re.match(r'^\d+$', os.environ['outjson'])):
    outjson = int( os.environ['outjson'] )
#
# Filter query by gameid
#
# Example usage1:   gamefilter=1229 python3 listcc.py
#       Finds SMW Streams with extension active
# Example usage2:   outjson=0 gamefilter='&game_id=1229&game_id=1036710512' python3 listcc.py 
#       Lists both SMW and Palworld streams with extension active
#
#
# Example usage3:   gamefilter=1036710512  python3 listcc.py 
#       Finds Palworld streams with extension active
#
gamefilter_query = ''
gameidlist = []
if ('gamefilter' in os.environ):
    if (re.search(r'&', os.environ['gamefilter'])):
        gamefilter_query = '&' + '&'.join( 
           filter(
              lambda attribute: re.match(r'^game_id=\d+', attribute), 
                map(
                     lambda g: '='.join(map(
                        lambda h: urllib.parse.quote(h),
                        g.split('='))),
                     os.environ['gamefilter'].split('&')
               )  #map
           ) #filter
        ) #join
    elif (os.environ['gamefilter'] == '1'):
        gamefilter_query = '&' + gameid
    elif (re.match(r'^\d+$', os.environ['gamefilter'])):
        gamefilter_query = f'&game_id={ os.environ["gamefilter"] }'
    for filterItem in gamefilter_query.split('&'):
        filterEntry = filterItem.split('=')
        if (filterEntry[0] == 'game_id'):
            gameidlist.append( str(filterEntry[1]) )
    print(f'GAMEFILTER_QUERY={gamefilter_query}')

if ('extension' in os.environ):
    if (re.search(r'&', os.environ['extension'])):
        extension_query = '' + '&'.join(
           filter(
              lambda attribute: re.match(r'^extension_id=[a-fA-F0-9]+', attribute),
                map(
                     lambda g: '='.join(map(
                        lambda h: urllib.parse.quote(h),
                        g.split('='))),
                     os.environ['extension'].split('&')
               )  #map
           ) #filter
        ) #join


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

while (first_page or (pagination_string and len(pagination_string) > 0)):
    first_page = False
    current_page = current_page + 1
    if current_page > maximum_pagecount:
         if not(outjson):
             print(f'[Display limited to {maximum_pagecount} pages, {maximum_pagecount*100} entries]')
         break
    if pagination_string:
        pagination_query = f'&after={urllib.parse.quote(pagination_string)}'
    else:
        pagination_query = ''

    lookup_url = ('https://api.twitch.tv/helix/extensions/live?'+
          extension_query + gamefilter_query + pagination_query)

    if not(outjson):
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
                if len(gameidlist) > 0:
                    j1["data"] = list(filter(lambda ei: str(ei["game_id"]) in gameidlist , j1["data"]))
                print(json.dumps(j1, indent=3))
            else:
                for item in j1["data"]: 
                    if (len(gameidlist) > 0):
                        if not( str(item["game_id"]) in gameidlist ):
                            continue
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

 















   

from bs4 import BeautifulSoup # web parser
import ctypes
import os # windows / linux commands
import platform
import re # regular expressions
import subprocess
import sys
import urllib
import xml.etree.ElementTree
import urllib2
import json
from xml.sax.saxutils import escape

##############################
#                            #
#          Modals            #
#                            #
##############################

class Payload(object):
    def __init__(self, rawJSON):
        self.__dict__ = json.loads(rawJSON)

##############################
#                            #
#          Functions         #
#                            #
##############################

def buildQuery(query):
    return "https://www.google.com/search?as_st=y&tbm=isch&as_q=%s&as_epq=&as_oq=&as_eq=&cr=&as_sitesearch=&safe=images&tbs=isz:lt,islt:vga,iar:s" % (query) 

def find(str, pattern):
    p = re.compile(pattern)
    r = re.findall(p, str)
    if (r and len(r) > 0):
        return r[0]
    else:
        return ""

# Exits with an error
def exitErr( str ):
   print "<Error>" + escape(str) + '</Error>'
   print "</Output>"
   sys.exit(0)

def downloadImage(query):
    site = buildQuery(query)
    hdr = {'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.64 Safari/537.11',
         'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
         'Accept-Charset': 'ISO-8859-1,utf-8;q=0.7,*;q=0.3',
         'Accept-Encoding': 'none',
         'Accept-Language': 'en-US,en;q=0.8',
         'Connection': 'keep-alive'}
    
    try:
        req = urllib2.Request(site, headers=hdr)
        page = urllib2.urlopen(req)

        content = page.read()

        soup = BeautifulSoup(content, "html.parser")
        linkarray = soup.find_all(attrs={"class": "rg_meta"})
        if len(linkarray) == 0:
            exitErr("No results found.")
        
        for div in linkarray:
            #try:
            rawJSON = div.decode_contents(formatter="json")
            data = Payload(rawJSON)
            url = data.ou.encode('ascii', 'ignore')
            height = data.oh
            width = data.ow
            type = data.ity
            title = data.pt.encode('ascii', 'ignore')
            description = data.s.encode('ascii', 'ignore')
                
            print "\t<Result>"
            print "\t\t<Title>%s</Title>" % escape(title)
            print "\t\t<Description>%s</Description>" % escape(description)
            print "\t\t<Url>%s</Url>" % escape(url)
            print "\t\t<Height>%d</Height>" % height
            print "\t\t<Width>%d</Width>" % width
            print "\t\t<Type>%s</Type>" % escape(type)
            print "\t</Result>"
            #except:
            #    continue
    except urllib2.HTTPError, e:
        exitErr(e.fp.read())

##############################
#                            #
#          MAIN              #
#                            #
##############################

print "<?xml version=\"1.0\" encoding=\"UTF-8\" ?>"
print "<Output>"

if len(sys.argv) < 2:
    exitErr("Number of arguments to script is invalid.\nUsage: python GetImageFromGoogle.py \"query\"")

query  = sys.argv[1]

# test:
#downloadImage("indie+recs++nov+2011+by+grouplove+by+butcher+boy+album+art")
downloadImage(query)

print "</Output>"

sys.exit(0)

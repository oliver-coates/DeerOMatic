import xml.etree.ElementTree as ET
import datetime
import pytz


class Placemark:
    
    def __init__ (self, e:ET.Element):
        self.name = "Unnamed Placemark"
        self.index = -1
        self.timeStamp = "Time not set"
        self.coordinates = (0.0, 0.0)

        self.SetupDataFromElement(e)

    
    def SetupDataFromElement(self, e:ET.Element):
        self.name = e[0].text

        # Grab all digits from the string to find the index (they are named like 'Marker12')
        if (self.name != None):
            self.index = int(''.join(char for char in self.name if char.isdigit()))
        
        # Get new zealand's timezone:
        utc = pytz.timezone("UTC")
        nzst = pytz.timezone("Pacific/Auckland")

        # Grab timestamp (which is in unix epoch time) & Parse it into datetime object:
        unix_timestamp = float(e[3][0][0].text) / 1000.0
        # unix_timestamp = 1774378013432 / 1000
        if (unix_timestamp != None):
            time = datetime.datetime.fromtimestamp(unix_timestamp)
            self.timeStamp = time.astimezone(nzst)
        
        # Grab coordinates
        coords = e[4][0].text
        if (coords != None):
            coords = coords.split(',')
        
            self.coordinates = (float(coords[0]), float(coords[1]))

    def __str__(self):
        return "[Placemark] " + str(self.name) + ", (Index: "+ str(self.index) +"), Time: " + str(self.timeStamp) + ", Coords: " + str(self.coordinates)
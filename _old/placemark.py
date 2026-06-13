import xml.etree.ElementTree as ET
import datetime
import dateutil


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
        
        # Grab timestamp & Parse it into datetime object:
        timeStrUTC = e[2][0].text
        if (timeStrUTC != None):
            time = dateutil.parser.parse(timeStrUTC)

        # Convert from UTC (which it is currently in) into NZST (difference of 12 hrs)
        nzst_timeDelta = datetime.timedelta(hours=12)
        nzst = datetime.timezone(nzst_timeDelta, name="NZST")
        if (nzst != None):
            self.timeStamp = time.astimezone(nzst)
        
        # Grab coordinates
        coords = e[4][0].text
        if (coords != None):
            coords = coords.split(',')
        
            self.coordinates = (float(coords[0]), float(coords[1]))

    def __str__(self):
        return "[Placemark] " + str(self.name) + ", (Index: "+ str(self.index) +"), Time: " + str(self.timeStamp) + ", Coords: " + str(self.coordinates)
import tkinter as tk
from tkinter import PhotoImage
import os
from tkinter import filedialog 
import placemark as p
import zipfile
import xml.etree.ElementTree as ET

def import_file():
    file_path = filedialog.askopenfilename(title="Select job data", filetypes=[("Keyhole Markup Language files", ["*.kml", "*.kmz"])])
    if (file_path):
        validate_then_parse_kml(file_path)


def validate_then_parse_kml(file_path:str):
    # .kml files are uncompressed and are all good to be parsed as is
    if (file_path.endswith(".kml")):
        tree = ET.parse(file_path)
        parse_kml(tree)
    # .kmz files need to be uncompressed
    elif (file_path.endswith(".kmz")):
        
        return

def parse_kml(tree:ET.ElementTree[ET.Element[str]]):
    root = tree.getroot()

    # Grab all the folders:
    folders = root[0].findall(path="{http://www.opengis.net/kml/2.2}Folder")
    
    # Find which of our folders is for 'Markers'
    marker_folder = 0
    for folder in folders:
        name = folder[0].text
        if (name == "Markers"):
            marker_folder = folder

    # Ensure we found the marker folder
    if (marker_folder == 0):
        print("ERROR: Could not find marker folder :(")
        return
    
    # Find the placemark folder (named 'glui') within the marker folder
    placemark_folder = 0
    for child_element in marker_folder:
        print(child_element)
        if (child_element.tag == "{http://www.opengis.net/kml/2.2}Folder"):
            folder_name = child_element[0].text
            print(folder_name)
            if (folder_name == "Glui"):
                placemark_folder = child_element

    # Ensure we found the placemark folder
    if (placemark_folder == 0):
        print("ERROR: Could not find placemark folder :(")
        return

    # Get all placemarks from the children of the placemark folder:
    placemarks = []
    for child_element in placemark_folder:
        if (child_element.tag == "{http://www.opengis.net/kml/2.2}Placemark"):
            placemarks.append(p.Placemark(child_element))
    
    for placemark in placemarks:
        print(placemark)



root = tk.Tk(screenName=None, baseName="GCH Deer-o-matic", className='GCH Deer-o-matic')

image = PhotoImage(file="resources/display.png")
image_label = tk.Label(root, image=image)
image_label.pack(pady=10)

headerLabel = tk.Label(root, text="Submit your data below:")
headerLabel.pack(pady=10)

importJobDataButton = tk.Button(root, text="Import Job Data", command=import_file)
importJobDataButton.pack(pady=10)

quitButton = tk.Button(root, text="Quit", command=root.destroy)
quitButton.pack(pady=10)

root.mainloop()
import tkinter as tk
from tkinter import PhotoImage
from tkinter import filedialog 
import tkinter.font as tkFont

from notifications import Notification
from pdf_exporter import export_form
import placemark as p
from hunt_info import HuntInfo

import json
import zipfile
import xml.etree.ElementTree as ET



def import_file():
    displayNotification(Notification("Importing file...", color=Notification.READY_COLOR))
    file_path = filedialog.askopenfilename(title="Select job data", filetypes=[("Keyhole Markup Language files", ["*.kml", "*.kmz"])])
    if (file_path):
        validate_then_parse_kml(file_path)

def validate_then_parse_kml(file_path:str):
    # .kml files are uncompressed and are all good to be parsed as is
    if (file_path.endswith(".kml")):
        try:
            tree = ET.parse(file_path)
        except Exception as e:
            displayNotification("Error while parsing kml: " + str(e), color=Notification.FAILURE_COLOR)
        parse_kml(tree)
    
    # Meanwhile, .kmz files need to be uncompressed
    elif (file_path.endswith(".kmz")):
        tree = None
        try:
            with zipfile.ZipFile(file_path, 'r') as unzipped_kmz:
                kml_raw = unzipped_kmz.read("doc.kml").decode()
                tree = ET.ElementTree(ET.fromstring(kml_raw))
        except Exception as  e:
            displayNotification("Error while parsing kmz: " + str(e), color=Notification.FAILURE_COLOR)
        if (tree != None):
            parse_kml(tree)
    
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
        displayNotification(Notification("KML parse error: Could not find marker folder.", Notification.FAILURE_COLOR))
        return
    
    # Find the placemark folder (named 'glui') within the marker folder
    placemark_folder = 0
    for child_element in marker_folder:
        if (child_element.tag == "{http://www.opengis.net/kml/2.2}Folder"):
            folder_name = child_element[0].text
            if (folder_name == "Glui"):
                placemark_folder = child_element

    # Ensure we found the placemark folder
    if (placemark_folder == 0):
        displayNotification(Notification("KML parse error: Could not find placemark folder.", Notification.FAILURE_COLOR))
        return

    # Get all placemarks from the children of the placemark folder:
    placemarks = []
    for child_element in placemark_folder:
        if (child_element.tag == "{http://www.opengis.net/kml/2.2}Placemark"):
            placemarks.append(p.Placemark(child_element))
    
    info = pullHuntInfo()
    n = export_form(placemarks, info)

    displayNotification(n)

def pullHuntInfo() -> HuntInfo:
    info = HuntInfo()

    info.hunter_name = hunter_name.get()
    info.hunter_id = hunter_id.get()
    info.other_hunter_names = other_hunter_names.get()
    info.animal_depot_or_rmp = animal_mat_depot.get()
    info.species_deer_hunted = species_deer_hunted.get()
    info.num_sticks_velvet = num_sticks_velvet.get()
    info.helicopter_reg = helicopter_registration.get()

    info.confirm_animals_procured_operations_manual = confirm_animals_procured_operations_manual.get()
    info.confirm_animals_no_poison_zone = confirm_animals_no_poison_zone.get()
    info.confirm_animals_no_disease = confirm_animals_no_disease.get()
    info.confirm_carcasses_below_mrl_mpl = confirm_carcasses_below_mrl_mpl.get()
    info.confirm_animals_no_chemicals = confirm_animals_no_chemicals.get()
    info.confirm_animals_no_contamination = confirm_animals_no_contamination.get()
    info.confirm_deer_no_tb = confirm_deer_no_tb.get()

    return info

def setupDefaultValues():
    defaults = None
    with open("settings/default_values.json", 'r') as file: 
        defaults = json.load(file)
    
    if (defaults == None):
        return

    hunter_name.set(defaults["Hunter Name"])
    hunter_id.set(defaults["Hunter ID"])
    other_hunter_names.set(defaults["Other Hunter Names"])
    animal_mat_depot.set(defaults["Depot or RMP Identifier"])
    species_deer_hunted.set(defaults["Deer species hunted"])
    num_sticks_velvet.set(defaults["Number sticks of velvet"])
    helicopter_registration.set(defaults["Helicopter Registration"])

def displayNotification(n:Notification):
    status_label.configure(text=n.message, background=n.color)

# Tkinter root:
root = tk.Tk(screenName=None, baseName="GCH Deer-o-matic", className=" GCH Deer-o-matic")

# Form Info:
hunter_name = tk.StringVar()
hunter_id = tk.StringVar()
other_hunter_names = tk.StringVar()
animal_mat_depot = tk.StringVar()
species_deer_hunted = tk.StringVar()
num_sticks_velvet = tk.StringVar()
helicopter_registration = tk.StringVar()

confirm_animals_procured_operations_manual = tk.BooleanVar(value=True)
confirm_animals_no_poison_zone = tk.BooleanVar(value=True)
confirm_animals_no_disease = tk.BooleanVar(value=True)
confirm_carcasses_below_mrl_mpl = tk.BooleanVar(value=True)
confirm_animals_no_chemicals = tk.BooleanVar(value=True)
confirm_animals_no_contamination = tk.BooleanVar(value=True)
confirm_deer_no_tb = tk.BooleanVar(value=True)

setupDefaultValues()

# Tkinter GUI:
header_font = tkFont.Font(family="Arial", size = 18)

headerLabel = tk.Label(root, text="Deer-o-matic 9000", font=header_font)
headerLabel.pack(pady=2.5)

image = PhotoImage(file="resources/display.png")
image_label = tk.Label(root, image=image)
image_label.pack(pady=5)

tk.Label(root, text = "Call Oliver for tech support ( +64 021 241 1221 )").pack()

# Hunt info:
hunt_info_frame = tk.Frame(root, borderwidth=2, relief=tk.RIDGE)
hunt_info_frame.pack(padx=10, pady=5)

tk.Label(hunt_info_frame, text="Hunter Name", padx=2.5).grid(row=0, column=0, sticky='e')
hunter_name_field = tk.Entry(hunt_info_frame, width = 30, textvariable=hunter_name).grid(row=0, column=1)

tk.Label(hunt_info_frame, text="Hunter ID", padx=2.5).grid(row=0, column=2, sticky='e')
tk.Entry(hunt_info_frame, width=30,textvariable=hunter_id).grid(row=0, column=3)

tk.Label(hunt_info_frame, text="Names of other hunters", padx=2.5).grid(row=1, column=0, sticky='e')
tk.Entry(hunt_info_frame, width=30, textvariable=other_hunter_names).grid(row=1, column=1)

tk.Label(hunt_info_frame, text="Depot/RMP Identifier", padx=2.5).grid(row=1, column=2, sticky='e')
tk.Entry(hunt_info_frame, width=30, textvariable=animal_mat_depot).grid(row=1, column=3)

tk.Label(hunt_info_frame, text="Deer species hunted", padx=2.5).grid(row=2, column=0, sticky='e')
tk.Entry(hunt_info_frame, width=30, textvariable=species_deer_hunted).grid(row=2, column=1)

tk.Label(hunt_info_frame, text = "Num. sticks of velvet", padx=2.5).grid(row=2, column=2, sticky='e')
tk.Entry(hunt_info_frame, width=30, textvariable=num_sticks_velvet).grid(row=2, column=3)

tk.Label(hunt_info_frame, text="Helicopter reg.", padx=2.5).grid(row=3, column=0, sticky='e')
tk.Entry(hunt_info_frame, width=30, textvariable=helicopter_registration).grid(row=3, column=1)

additional_questions_frame = tk.Frame(root, borderwidth=2, relief=tk.RIDGE)
additional_questions_frame.pack(padx=10, pady=5)

# Additional questions form:
tk.Label(additional_questions_frame, text="Can you confirm...").grid(row=0, column=0, sticky='w')

tk.Label(additional_questions_frame, text="That these animals were procured in accordance with your Operations Manual?",).grid(row=1, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_animals_procured_operations_manual).grid(row=1, column=1)

tk.Label(additional_questions_frame, text="That none of the animals have been recovered from poisoned\nland or buffer zones within the applicable caution periods?").grid(row=2, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_animals_no_poison_zone).grid(row=2, column=1)

tk.Label(additional_questions_frame, text="That the animals when live, and their carcasses, were free from visible signs of illness or disease?").grid(row=3, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_animals_no_disease).grid(row=3, column=1)

tk.Label(additional_questions_frame, text="To the best of your knowledge, that the carcasses are below the MRL and MRP?").grid(row=4, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_carcasses_below_mrl_mpl).grid(row=4, column=1)

tk.Label(additional_questions_frame, text="To the best of your knowledge, that these animals had not ingested agricultural\nchemicals and are outside the witholding period for any veterinary medicines?").grid(row=5, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_animals_no_chemicals).grid(row=5, column=1)

tk.Label(additional_questions_frame, text="That the carcasses, while under your control, were maintained under conditions\nthat minimise contamination and deterioration, and not frozen?").grid(row=6, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_animals_no_contamination).grid(row=6, column=1)

tk.Label(additional_questions_frame, text="That all deer covered by this declaration, were killed in Tb vector free areas?").grid(row=7, column=0, sticky='w', pady=2.5)
tk.Checkbutton(additional_questions_frame, variable=confirm_deer_no_tb).grid(row=7, column=1)

status_label = tk.Label(root, text = "Status: Ready", borderwidth=2, relief=tk.RIDGE)
status_label.pack(pady=2.5)

# Bottom frame:
bottom_frame = tk.Frame(root)
bottom_frame.pack(padx=10, pady=5)

importJobDataButton = tk.Button(bottom_frame, text="Import Job Data & Export", command=import_file)
importJobDataButton.grid(row=0, column=1, padx=5)

quitButton = tk.Button(bottom_frame, text="Quit", command=root.destroy)
quitButton.grid(row=0, column=2, padx=5)


# Ready:
displayNotification(Notification("Ready!", color=Notification.READY_COLOR))


root.mainloop()
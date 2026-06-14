import placemark as p
from notifications import Notification
from datetime import datetime
import math
from hunt_info import HuntInfo
import os
from fillpdf import fillpdfs


# Config:
OUTPUT_FILE_PREFIX = "14743-LHSD"


def export_form(placemarks:list[p.Placemark], info:HuntInfo) -> Notification:
    try:
        # Base and additional fill data is pulled easily from 
        base_fill_data = getBaseFillData(placemarks, info)

        # Note we are assuming all placemarks are valid marks. Consider checking if we have dud placemarks in future.
        num_placemarks = len(placemarks)
        num_docs = math.ceil(num_placemarks / 4)
        placemark_index = 0

        # Iterate over loop for each document we need to produce and output it.
        for document_index in range(0, num_docs):
            # The fill data is the base fill data, plus the four rows of data from the four placemarks
            doc_fill_data = base_fill_data
            # Loop for each row in the form:
            for row_index in range(0, 4):
                # If we have a num of placemarks that is not divisible by four, (i.e. 5),
                # We will skip the remaining iterations, (so the final form will only have the 1st row filled out in the case of 5 placemarks)
                if (placemark_index >= num_placemarks):
                    continue
                
                # Pull the placemark fill data at the index of placemarks, note the row index is in range 0..3
                placemark_data = getPlacemarkFillData(placemarks[placemark_index], row_index)
                placemark_index += 1

                # Combine each placemark dict data together
                doc_fill_data = doc_fill_data | placemark_data

            # Finally output this document with the combined fill data.
            output(doc_fill_data, document_index)

        return Notification("Success! Exported " + str(num_docs) + " documents to the output folder.", color=Notification.SUCCESS_COLOR)
    except Exception as e:
        return Notification("Failure while exporting pdf: " + str(e), color=Notification.FAILURE_COLOR)

def output(fill_data:dict, index:int):
        # The name of the pdf is the file prefix, plus the year-month-day, plus the document index
        exported_pdf_name = OUTPUT_FILE_PREFIX + datetime.today().strftime("-%y-%m-%d") + "-" + str(index+1)
        
        # We need to export a writable file temporarily so we can flatten it using the flatten_pdf method.
        temp_export_file_path = "output/"+str(exported_pdf_name)+ "-writable_temp.pdf"
        export_file_path = "output/"+str(exported_pdf_name)+".pdf"

        # First we create this temporary fillable pdf:
        fillpdfs.write_fillable_pdf("resources/14743-LHSD_fillable3.pdf", temp_export_file_path, fill_data, flatten=False)
        # Then we flatten it to the export file path:
        fillpdfs.flatten_pdf(temp_export_file_path, export_file_path, as_images=False)
        # Finally delete this temp file:
        os.remove(temp_export_file_path)

def getBaseFillData(placemarks:list[p.Placemark], info:HuntInfo) -> dict:
    base = getGenericFillData(placemarks, info)
    additional = getAdditionalQuestionsFillData(info)
    
    return base | additional


def getGenericFillData(placemarks:list[p.Placemark], info:HuntInfo) -> dict:
    hunter_name_and_id = info.hunter_name + " " + info.hunter_id

    other_hunters = info.other_hunter_names
    
    animal_material_depot = info.animal_depot_or_rmp

    fridge_date = placemarks[0].timeStamp.strftime("%d/%m/%y")
    
    num_animals = len(placemarks)
    animal_type = info.species_deer_hunted
    num_velvet = info.num_sticks_velvet
    num_and_species = str(num_animals) + " " + animal_type + ", " + str(num_velvet) + " sticks of velvet"

    helicopter_registration = info.helicopter_reg

    todays_date = datetime.today().strftime("%d/%m/%y")

    fill_data = {
        "text-hunter-name" : hunter_name_and_id,
        "text-other-hunters" : other_hunters,
        "text-animal-material-depot" : animal_material_depot,
        "date-date-of-arrival" : fridge_date,
        "text-number-and-species" : num_and_species,
        "text-helicopter-registration" : helicopter_registration,
        "text-date-signed" : todays_date
    }

    return fill_data

def getPlacemarkFillData(placemark:p.Placemark, columnIndex:int) -> dict:
    field_carcass_id = "text-carcass-id-" + str(columnIndex)
    field_kill_location = "text-kill-location-" + str(columnIndex)
    field_date_time_killed = "date-date-time-killed-" + str(columnIndex)
    field_date_time_fridge = "date-date-and-time-fridge-" + str(columnIndex)
    
    time_killed = placemark.timeStamp.strftime("%d/%m/%y; %H:%M")

    fill_data = {
        field_carcass_id : str(placemark.index),
        field_kill_location : format_coordinates(placemark.coordinates),
        field_date_time_killed : time_killed,
        field_date_time_fridge : ""
    }

    return fill_data

def getAdditionalQuestionsFillData(info:HuntInfo) -> dict:
    confirm_animals_procured_operations_manual = "bool-procured-accordance-operations-manual-" + str(format_question_bool(info.confirm_animals_procured_operations_manual))
    confirm_animals_no_poison_zone = "bool-recovery-none-poisoned-" + str(format_question_bool(info.confirm_animals_no_poison_zone))
    confirm_animals_no_disease = "bool-animals-no-disease-" + str(format_question_bool(info.confirm_animals_no_disease)) 
    confirm_carcasses_below_mrl_mpl = "bool-below-mrl-mpl-" + str(format_question_bool(info.confirm_carcasses_below_mrl_mpl))
    confirm_animals_no_chemicals = "bool-no-chemicals-medicine-" + str(format_question_bool(info.confirm_animals_no_chemicals))
    confirm_animals_no_contamination = "bool-carcasses-good-conditions-" + str(format_question_bool(info.confirm_animals_no_contamination))
    confirm_deer_no_tb = "bool-deer-tb-free-" + str(format_question_bool(info.confirm_deer_no_tb))


    fill_data = {
        confirm_animals_procured_operations_manual : "Yes",
        confirm_animals_no_poison_zone : "Yes",
        confirm_animals_no_disease : "Yes",
        confirm_carcasses_below_mrl_mpl : "Yes",
        confirm_animals_no_chemicals : "Yes",
        confirm_animals_no_contamination : "Yes",
        confirm_deer_no_tb : "Yes"
    }

    return fill_data

def format_question_bool(b:bool) -> str:
    if (b):
        return "yes"
    else:
        return "no"

def format_coordinates(coords:tuple[int, int]) -> str:
    return "" + str(coords[1]) + ', ' + str(coords[0])
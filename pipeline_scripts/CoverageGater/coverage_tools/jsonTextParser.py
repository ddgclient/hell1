"""Module handling the parsing of JSON text"""
import json

def correctJSONText(jsonText: str):
    """Peforms some post processing of text. For some reason, dotCover 
    adds these weird latin symbols to the start of their file that
    needs to be removed, if they're left in, they'll cause the 
    conversion to JSON object to fail."""
    maxIndex = len(jsonText) - 1
    i = 0

    while i < maxIndex:
        if jsonText[i] == '{':
            return jsonText[i:maxIndex+1]
        i += 1

def createJSONObject(filepath):
    jsonText = ''
    with open(filepath, 'r') as reader:
        jsonText = reader.read()
    
    correctedText = correctJSONText(jsonText)
    jsonObject = json.loads(correctedText)
    return jsonObject
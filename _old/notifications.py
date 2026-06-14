

class Notification:  
    DEFAULT_COLOR = "#7c7c7b" 
    READY_COLOR = "#cec84c"
    FAILURE_COLOR = "#c12a2a"
    SUCCESS_COLOR = "#2caa3b"

    def __init__(self, message:str, color:str = DEFAULT_COLOR):
        self.message = message
        self.color = color
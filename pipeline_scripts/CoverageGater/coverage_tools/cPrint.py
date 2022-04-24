"""Module that allows for printing to console w/ pretty colors :). 
Code taken from:
https://www.geeksforgeeks.org/print-colors-python-terminal/"""

def cRed(skk): print("\033[91m {}\033[00m" .format(skk))
def cGreen(skk): print("\033[92m {}\033[00m" .format(skk))
def cYellow(skk): print("\033[93m {}\033[00m" .format(skk))
def cLightPurple(skk): print("\033[94m {}\033[00m" .format(skk))
def cPurple(skk): print("\033[95m {}\033[00m" .format(skk))
def cCyan(skk): print("\033[96m {}\033[00m" .format(skk))
def cLightGray(skk): print("\033[97m {}\033[00m" .format(skk))
def cBlack(skk): print("\033[98m {}\033[00m" .format(skk))
#import requests
import sys, json, os

vc4_server_ip = ''
base_url = f'http://{vc4_server_ip}/VirtualControl/ProgramLibrary'

def help():
    print('Usage: python main.py path update startnow')

if __name__ == '__main__':
    if len(sys.argv) < 2:
        help()

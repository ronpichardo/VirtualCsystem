import requests
import sys, json, os

vc4_server_ip = ''
base_url = f'http://{vc4_server_ip}/VirtualControl/config/api'
vc4_apikey = None
cpz_filepath = ''

def help():
    print('Usage: python main.py getrooms|getprogs|updateprog [path] [programId]')
    sys.exit(0)

def get_programs():
    route_url = f'{base_url}/ProgramInstance'
    headers = {
        'accept': 'application/json',
        'Authorization': vc4_apikey
    }
    res = requests.get(route_url, headers=headers)
    print(res.content)

def get_rooms():
    route_url = f'{base_url}/ProgramInstance'
    headers = {
        'accept': 'application/json',
        'Authorization': vc4_apikey
    }
    res = requests.get(route_url, header=headers)
    print(res.content)

def update_prog(id=1):
    route_url = f'{base_url}/ProgramLibrary'
    headers = {
        'accept': 'application/json',
        'Authorization': vc4_apikey,
        'Content-Type': 'multipart/form-data'
    }
    form_data = {
        'ProgramId': id,
        'FriendlyName': 'TBD',
        'Notes': 'Notes about room'
    }

    program_file = {
        'AppFile': open(cpz_filepath, 'rb')
    }

    res = requests.post(route_url, headers=headers, files=program_file, data=form_data)
    print(res.content)
    
if __name__ == '__main__':
    if len(sys.argv) < 2:
        help()
    
    print(sys.argv)
    if len(sys.argv) == 2:
        if sys.argv[1] == 'getrooms':
            get_rooms()
        elif sys.argv[1] == 'getprogs':
            get_programs()
    elif len(sys.argv) == 3:
        print('No program ID was provided, default value of 1 will be used')
        are_u_sure = input('Continue with default ID of 1? y or n').lower()
        if are_u_sure == 'yes' or are_u_sure == 'y':
            cpz_filepath = sys.argv[2]
        else:
            sys.exit(0)
    elif len(sys.argv) == 4:
        if sys.argv[1] == 'updateprog':
            cpz_filepath = sys.argv[2]
            if
            prog_id = sys.argv[3]
            print(cpz_filepath)
    else:
        help() 
    

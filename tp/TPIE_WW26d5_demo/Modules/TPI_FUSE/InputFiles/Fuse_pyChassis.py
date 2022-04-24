'''
    ================================================================

    Sample Python Chassis. This example would be called with:

    function_name = "EmbPython!Execute"
    function_parameter = "~OASIS_TPL_DIR/Modules/S_DUMMY_MODULE/InputFiles/PyChassis -Functions external_1"

    ================================================================
'''

# Module Imports (Warning: Evergreen EmbPython supports _VERY_FEW_ Modules !!! )
#import os
import sys

# To Use a single directory for the evg Simulator, you can Edit & Uncomment
# the Line below, to load evg.py from a fixed location.

# For Simulation ONLY !!
# Folder that contains the evg.py file
# sys.path.append(os.path.abspath('C:/Users/pwmalone/PycharmProjects/EVG'))

# Production
import evg

# Tags
__author__ = 'pwmalone'
__email__ = 'patrick.w.maloney@intel.com'
__revision__ = 'R1.5'

'''
    ================================================================
                         Globals & Constants
    ================================================================
'''

# Ports
STD_PASS_PORT = 1
STD_FAIL_PORT = 0
STD_FAIL_BITS_PORT = 2
STD_EXCPT_PORT = -1
STD_CLAMP_PORT = -2
STD_UNDEF_PORT = -99

# Debug Mode to Logging Level Lookup
BYPASS_BUFFER_LEVEL = -99
STD_DEBUG_MODE = {'DISABLED': 0, 'BRIEF': 1, 'VERBOSE': 2, 'OBNOXIOUS': 3}

# Some UserVars
STD_MIDAS_ENABLED_USRV = 'CorTeXGlobals.iCGL_EnableMIDAS'
STD_PUDL_ENABLED_USRV = 'CorTeXGlobals.iCGL_EnablePUDLLogtoFile'


iSsid = -99


'''
    ================================================================
                              Classes
    ================================================================
'''


class ObjIni():
    '''
            Ini Config File Class
    '''

    # Class Constants

    def __init__(self, s_ini_file):
        #
        self._filename_ = s_ini_file
        self._d_params_ = {}
        # Call Parser
        self.parse_file()

    def parse_file(self):
        '''
            Parses specified ini file, & populates Class Structured Dictionary
        '''

        # Local Variables
        s_section = ''
        s_subsection = ''
        d_params = {}

        try:
            # Copy File To Site Controller
            s_ini_file = evg.GetFile(self._filename_)

            f = open(s_ini_file, 'r')
            s_line = f.readline()
            while len(s_line) > 0:
                # Remove Leading & Trailing Whitespace
                s_striped_line = s_line.replace('\n', '').replace('\r', '').strip()

                # Skip Comment  & Blank Lines
                if len(s_striped_line) > 0 and s_striped_line[0] != '#':
                    # Check to see if Line is a Section Header i.e. [Something]
                    if s_striped_line[0] == '[':
                        # Strip square brackets off line & grep out Section name.
                        l_fields = s_striped_line.replace('[', '').replace(']', '').split()
                        s_section = l_fields[0]

                        # Initialise Section name in Dictionary, if necessary
                        if s_section not in d_params.keys():
                            d_params[s_section] = {}

                        # Reset SubSection
                        s_subsection = ''
                    elif s_striped_line[0] == '_':
                        # Line looks like a Sub-Section Header
                        s_subsection = s_striped_line[1:-1]

                        # Initialise Sub-Section, within Section, in Dictionary, if necessary
                        if s_subsection not in d_params[s_section].keys():
                            d_params[s_section][s_subsection] = {}
                    else:
                        # Line should be a Parameter/Value Pair for the Current Section if it isn't zero length !
                        l_fields = s_striped_line.split('=')

                        # Replace certain critial characters to Protect them
                        if '::' in l_fields[1]:
                            l_fields[1] = l_fields[1].replace('::', '<DOUBLECOLON>')

                        # If Section & Sub-Section Specified
                        if s_subsection != '' and s_section != '':
                            # Check if it looks like a Dictionary
                            if ':' in l_fields[1]:
                                d_param = {}
                                l_dict = l_fields[1].split(',')
                                for l_entry in l_dict:
                                    [s_key, s_value] = l_entry.split(':')
                                    # Replace colons removed before
                                    s_key = s_key.replace('<DOUBLECOLON>', '::')
                                    s_value = s_value.replace('<DOUBLECOLON>', '::')

                                    if s_value == '{}':
                                        # Dict
                                        d_param[s_key] = {}
                                    elif s_value == '[]':
                                        # List
                                        d_param[s_key] = []
                                    else:
                                        d_param[s_key] = s_value

                                d_params[s_section][s_subsection][l_fields[0]] = d_param
                            elif '{}' == l_fields[1]:
                                # Empty Dictionary
                                d_params[s_section][s_subsection][l_fields[0]] = {}
                            elif '[]' == l_fields[1]:
                                # Empty List
                                d_params[s_section][s_subsection][l_fields[0]] = []
                            else:
                                # If it isn't a Dictionary, check does it look like a List, i.e. Comma seperated values
                                l_fields[1] = l_fields[1].replace('<DOUBLECOLON>', '::')
                                if ',' in l_fields[1]:
                                    s_last_char = l_fields[1][len(l_fields[1])-1]
                                    l_values = l_fields[1].split(',')

                                    if s_last_char == ',':
                                        # Forced List, as last Char is a Comma
                                        d_params[s_section][s_subsection][l_fields[0]] = []
                                        for s_value in l_values:
                                            if len(s_value.strip()) > 0:
                                                d_params[s_section][s_subsection][l_fields[0]].append(s_value)
                                    else:

                                        d_params[s_section][s_subsection][l_fields[0]] = l_values
                                else:
                                    d_params[s_section][s_subsection][l_fields[0]] = l_fields[1]

                        # No Sub-Section Specified
                        if s_section != '' and s_subsection == '':
                            # Check if it looks like a Dictionary
                            if ':' in l_fields[1]:
                                d_param = {}
                                l_dict = l_fields[1].split(',')
                                for l_entry in l_dict:
                                    [s_key, s_value] = l_entry.split(':')
                                    # Replace colons removed before
                                    s_key = s_key.replace('<DOUBLECOLON>', '::')
                                    s_value = s_value.replace('<DOUBLECOLON>', '::')

                                    if s_value == '{}':
                                        # Dict
                                        d_param[s_key] = {}
                                    elif s_value == '[]':
                                        # List
                                        d_param[s_key] = []
                                    else:
                                        d_param[s_key] = s_value

                                d_params[s_section][l_fields[0]] = d_param
                            elif '{}' == l_fields[1]:
                                # Empty Dictionary
                                d_params[s_section][l_fields[0]] = {}
                            elif '[]' == l_fields[1]:
                                # Empty List
                                d_params[s_section][l_fields[0]] = []
                            else:
                                # If it isn't a Dictionary, check does it look like a List, i.e. Comma seperated values
                                l_fields[1] = l_fields[1].replace('<DOUBLECOLON>', '::')
                                if ',' in l_fields[1]:
                                    s_last_char = l_fields[1][len(l_fields[1])-1]
                                    l_values = l_fields[1].split(',')

                                    if s_last_char == ',':
                                        # Forced List, as last Char is a Comma
                                        d_params[s_section][l_fields[0]] = []
                                        for s_value in l_values:
                                            if len(s_value.strip()) > 0:
                                                d_params[s_section][l_fields[0]].append(s_value)
                                    else:
                                        d_params[s_section][l_fields[0]] = l_values
                                else:
                                    # Simple Parameter
                                    d_params[s_section][l_fields[0]] = l_fields[1]
                        else:
                            #print '[ERROR] Not Section name specified for Line:[%s]' % s_striped_line
                            pass
                # Get Next Line
                s_line = f.readline()

            f.close()

            # Assign Local Dictionary to Class Dictionary
            self._d_params_ = d_params

        except Exception:
            raise_exception('[ERROR] Exception thrown during parse_file() method')

    def params(self):
        return self._d_params_


'''
    ================================================================
                         Chassis Functions
    ================================================================
'''


def check_is_midas_enabled():
    '''
        Check to see if MIDAS Datalogging is enabled, as this allows 0_* ituff lines

    :return: boolean True/False = Enabled/Disabled
    '''

    # Local Variables
    b_midas_enabled = False

    s_value = evg.GetTpGlobalValue(STD_MIDAS_ENABLED_USRV, 'string')
    if s_value.upper == 'TRUE':
        # 0_tname... etc allowed
        b_midas_enabled = True

    return b_midas_enabled


def log_to_datalog(n_level, s_ituff):
    '''
        Add specified sting to Datalog buffer. To Skip buffer & Write immediately, specify
        Level = -99

    :param n_level: Logging Level i.e. 0=Disabled, 1=Brief, 2=Verbose, 3=Obnoxious
    :param s_ituff: String to Add to Datalog Buffer
    :return:
    '''

    # Global Variables
    global g_l_datalog

    # Local Variables
    b_midas_enabled = check_is_midas_enabled()

    # Initialise List on first write
    if not len(g_l_datalog):
        g_l_datalog = []

    # Split String by \n, so we can check each line has the correct level i.e. 2_ 3_ etc
    l_lines = s_ituff.split('\n')
    for s_line in l_lines:
        if len(s_line) > 0:
            s_level = s_line[0]
            # If level '0_' make sure MIDAS is enabled !
            if not b_midas_enabled and s_level == '0':
                # Barf
                raise_exception('[ERROR] Invalid ITUFF String [%s], If MIDAS is Disabled !\n' % s_line)

            # Make Sure String has Carriage Return
            if s_line[-1] != '\n':
                s_line += '\n'
            # Create Tuple & Add to List ( If -99 is Level, don't just buffer, but write immediately )
            if n_level > BYPASS_BUFFER_LEVEL:
                # Buffer Message
                g_l_datalog.append((n_level, s_line))
            else:
                # Write Immediately
                evg.PrintToItuff(s_line)


def write_datalog_buffer(n_level):
    '''
        Write all lines in Datalog buffer, below specified Level, to Datalog

    :param n_level: Logging Level below which to log i.e. 0=Disabled, 1=Brief, 2=Verbose, 3=Obnoxious
    :return:
    '''

    # Local Variables
    s_ituff = ''

    # Add all lines, below specified Level, to Ituff string
    for t_entry in g_l_datalog:
        if t_entry[0] <= n_level and t_entry[0] > BYPASS_BUFFER_LEVEL:
            s_ituff += t_entry[1]

    # Write final string to Ituff
    evg.PrintToItuff(s_ituff)


def clear_datalog_buffer():
    '''

    :return:
    '''

    g_l_datalog = []


def log_to_console(n_level, s_console):
    '''
         Add specified sting to Console buffer. To Skip buffer & Write immediately, specify
        Level = -99

    :rtype : object
    :param n_level: Logging Level i.e. 0=Disabled, 1=Brief, 2=Verbose, 3=Obnoxious
    :param s_console: String to Add to Console Buffer
    :return:
    '''

    # Global Variables
    global g_l_console

    # Initialise List on first write
    if not len(g_l_console) > 0:
        g_l_console = []

    # Make Sure String has Carriage Return
    if len(s_console) > 0 and s_console[-1] != '\n':
        s_console += '\n'

    # Create Tuple & Add to List ( If -99 is Level, don't just buffer, but write immediately )
    if n_level > BYPASS_BUFFER_LEVEL:
        # Buffer Message
        g_l_console.append((n_level, s_console))
    else:
        # Write Immediately
        evg.PrintToConsole(s_console)


def write_console_buffer(n_level=None):
    '''
        Write all lines in Console buffer, below specified Level, to Console
        If Level isn't specified, the debug_mode is used as the verbosity level

    :param n_level: Logging Level below which to log i.e. 0=Disabled, 1=Brief, 2=Verbose, 3=Obnoxious
    :return:
    '''
    # Local Variables
    s_console = ''

    # Get Logging Level, from instance, if not specified
    if n_level is None:
        s_debug_mode = evg.GetTestParam(evg.GetInstanceName(), 'debug_mode')
        n_level = STD_DEBUG_MODE[s_debug_mode]

    # Add all lines, below specified Level, to Console string
    for t_entry in g_l_console:
        if t_entry[0] <= n_level and t_entry[0] > BYPASS_BUFFER_LEVEL:
            s_console += t_entry[1]

    # Write final string to Console
    evg.PrintToConsole(s_console)


def clear_console_buffer():
    '''

    :return:
    '''

    g_l_console = []


def update_exit_port(n_port):
    '''
        ============================================================
        Function to update exit port, unless port is already set to
        a fail Port, i.e. Port# <= 0, in which case protect keep
        fail port setting.
        ============================================================
    '''

    # GOTCHA ! - If you don't tell python to use the existing global variable, it'll throw exceptions :(
    global exit_port

    # Local Variables
    old_port = exit_port

    try:

        #if debug:
            #print '[DEBUG] Trying to update Exit Port from Port#:[%d] to Port#:[%d]' % (old_port, n_port)

        # If new port is same as existing port, do nothing
        if n_port != exit_port:
            # No Port Set, so just set Port as specified
            if exit_port == STD_UNDEF_PORT:
                #if debug:
                    #print '[DEBUG] Setting first Port#:[%d]' % n_port
                exit_port = n_port
            else:
                # A Port is already Set, so decide if we update
                if n_port < STD_PASS_PORT:
                    # New Port is a Fail Port, so lowest port wins
                    if n_port < exit_port:
                        #if debug:
                            #print '[DEBUG] Setting new Fail Port#:[%d], from Port#:[%d]' % (n_port, old_port)
                        exit_port = n_port
                else:
                    # New Port is pass Port, so latest port wins
                    #if debug:
                        #print '[DEBUG] Setting new Pass Port#:[%d], overriding old Port#:[%d]' % (n_port, old_port)
                    exit_port = n_port
        #else:
            #if debug:
                #print '[DEBUG] New Port#:[%d], is same as existing Port#:[%d], so doing nothing !' % (n_port, old_port)

    except Exception:
        s_exp_msg = '[ERROR] Exception thrown while executing update_exit_port()'
        evg.PrintToConsole(s_exp_msg)
        evg.SetSitePort(0, STD_EXCPT_PORT)
        raise ValueError(s_exp_msg)


def parse_cmd_line():
    '''
        ============================================================
        Command Line Parser to Return Dictionary of Command Line
        Switches & Values
        ============================================================
    '''

    # Globals (Mirror externally referenced globals)
    global debug

    # Local Variables
    d_args = {}
    d_tmp = {}
    switch = ''

    try:
        # Read Command Line - Use the EVG Call in Real Scripts :)
        full_cmd_line = evg.GetUFArgument()
        cmd_line_args = " ".join(full_cmd_line.split()[1:])
        l_args = cmd_line_args.split()

        # Loop thru Arguments
        for arg in l_args:
            # Check if arg is a switch, or a value
            if arg[0] == '-' or arg[0] == '+':
                # Switch
                switch = "%s%s" % (arg[0], arg[1:].upper())
                if switch in d_tmp.keys():
                    # Already Present, so Overwrite existing Settings
                    d_tmp[switch].append(arg)
                else:
                    # Initialize
                    d_tmp[switch] = []
            else:
                # Must be a Value, but only if a Switch is active
                if switch != '':
                    d_tmp[switch].append(arg)

        # Loop Thru Temporary Dictionary & Refactor it based on Style of Input to Bool, Dict, or Lists
        for switch in d_tmp.keys():
            if len(d_tmp[switch]) == 0:
                # Find Flags, i.e. Switches without values, like -Debug, or +Datalog
                # Respond to Signs, with '-' == False, & '+' == True
                if switch[0] == '+':
                    d_args[switch[1:]] = True
                elif switch[0] == '-':
                    d_args[switch[1:]] = False
            elif len(d_tmp[switch]) == 1:
                # Find Switches with a single values, like -Config
                d_args[switch[1:]] = d_tmp[switch][0]
            elif len(d_tmp[switch]) > 1:
                # Find Switches with a multiple values, like -CMEM
                d_args[switch[1:]] = d_tmp[switch]

        # Print UF Arguments Dictionary, if debugging is enabled
        if 'DEBUG' in d_args.keys():
            if d_args['DEBUG']:

                # Update debug Variable, if specified in UF Arguments
                debug = d_args['DEBUG']
                #print '[DEBUG] Command Line Arguments are:'
                #for key in d_args.keys():
                #    print "[DEBUG] Switch:[%s]=[%s]" % (key, d_args[key])
            else:
                # Default to debug off, if not passed in as an argument
                debug = False

        return d_args

    except Exception:
        s_exp_msg = '[ERROR] Exception thrown while executing parse_cmd_line()'
        evg.PrintToConsole(s_exp_msg)
        update_exit_port(STD_EXCPT_PORT)
        raise ValueError(s_exp_msg)


def raise_exception(s_exp_msg):
    '''
        Take Incoming Message & Raise And Exception & set Exit Port to -1

    :param s_exp_msg:
    :return: None
    '''
    evg.PrintToConsole(s_exp_msg)
    update_exit_port(STD_EXCPT_PORT)
    raise ValueError(s_exp_msg)


def get_flow():
    '''
            Determine current Socket & whether we're at Sort, or Class

    :return: Test Flow, i.e. 'SORT' or 'CLASS'
    '''


    '''
        We have to put 'GetTpGlobalValue' inside a try branch, as the collection SCVars only exists in Class
        & the collection NTSCVars only exists in SORT, so in the other location the call will result in an
        exception !
    '''
    try:
        # By Default, assume we're in SORT
        s_process_step = evg.GetTpGlobalValue('NTSCVars.SC_TEST_FLOW', 'string')
    except:
        # Otherwise, we're probably in Class ;)
        s_process_step = evg.GetTpGlobalValue('SCVars.SC_TEST_FLOW', 'string')

    return s_process_step.upper()


def main():
    '''
        ===============================================================
                    Default Function Called During Flow
        ===============================================================
    '''

    # Global Variables
    global uf_args
    uf_args = {}

    # List to Hold Tuples of Level,String to datalog
    global g_l_datalog
    g_l_datalog = []

    # List to Hold Tuples of Level,String to write to console
    global g_l_console
    g_l_console = []

    # Set Exit Port as Undefined initially, this should then get updated in the script
    global exit_port
    exit_port = STD_UNDEF_PORT

    # Global Debug variable (Updated from UF Arguments, if specified)
    global debug
    debug = False

    # Local Variables
    all_pass = True
    ret_value = False


    try:
        # Call Argument Parser, & return dictionary of switches & values
        uf_args = parse_cmd_line()

        # Call Function(s) Specified in Arguments, & get returned value
        if 'FUNCTIONS' in uf_args.keys():
            for func in uf_args['FUNCTIONS'].split(','):
                #if debug:
                    #print '[DEBUG] -----------------------------------------------------------------------------'
                    #print '[DEBUG] Calling Internal Function:[%s]' % func
                ret_value = eval(func+'()')

                # If any Function Called, returned a False, then set an overall fail
                if not ret_value:
                    all_pass = ret_value
        else:
            # No Functions switch with values in arguments, so no functions ran.
            #print '[ERROR] No \"-Functions\" switch in Command Line [%s] !' % evg.GetUFArgument()
            all_pass = False

        '''
            If everything goes ok, & the Exit Port has been updated somewhere, set Pass Exit Port, else fail.
        '''
        if exit_port == STD_UNDEF_PORT:
            # No Port set in Script, raise exception
            update_exit_port(STD_EXCPT_PORT)
            raise_exception('[ERROR] No Exit Port set anywhere !')
        else:
            '''
                Exit Port has been set somewhere. If all called Functions returned True, then Pass; if _ANY_ returned
                a False, then FAIL !
            '''
            if all_pass:
                pass
                #update_exit_port(STD_PASS_PORT)
            else:
                update_exit_port(STD_FAIL_PORT)

    except Exception:
        raise_exception('[ERROR] Exception thrown during Python UF')

    '''
        Set Instance Exit Port
    '''
    evg.SetSitePort(0, exit_port)

'''
    ================================================================
                        Internal Functions
    ================================================================
'''


def fuse_raster_tfile_header(sArrayName):
    '''
        Placeholder Internal Function. The Idea is that common functionality
        used by external functions should be broken out into a separate
        function.

    :return: True/False
    '''
    try:
        #print '[DEBUG] Calling fuse_raster_tfile_header() Function'

        # Local Variables




        #### Get X and Y Coordinates ####
        #print 'tfile name construct'
        #(tRetVal, sXCoordinate, sYCoordinate) = evg.TFileGetXYCoordinate()
        sXCoordinate = evg.GetTpGlobalValue('SCVars.SC_WAFERX', 'string')
        sYCoordinate = evg.GetTpGlobalValue('SCVars.SC_WAFERY', 'string')
        tRetVal = 1
        if tRetVal != 1:
            evg.PrintToConsole('failed tfile name construct')
            return False

        #### Check Tfile Open ####
    #        tRetVal = write_to_tfile_ok()
    # if tRetVal != 1:
    #     print 'failed tfile_ok check'
    #            return False

        ####Set up Array and Testname parameters
        #print 'set up parameters'
        #sArrayName = "Fuse_0_HVM, -10C,100ns,20us,M0L7M1L2,1.1/1.1,OFF,OFF,OFF,2.2/1.1,ENABLED,POR"
        sTestName = evg.GetInstanceName()
        #print sTestName

        ####Create Header String
        #sToWrite = "%s %s" % (sXcoordinate, sYCoordinate)
        sToWrite = "DUT %s, %s\nTest: %s\nArray: %s\n" % (sXCoordinate, sYCoordinate, sTestName, sArrayName)
        #print sToWrite

        ####Write to Tfile
        tRetVal = evg.TFileWrite(sToWrite)
        if tRetVal != 1:
            #print 'failed tfile header write'
            return False
        return True

    except Exception:
        raise_exception('[ERROR] Exception thrown during raster_tfile_header() function')
def CanaryMap(bitpos):
    try:
        #print 'In CanaryMap1'

        canarybit = [2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,140,141,142,143,144,145,146,147,148,149,150,151,152,153,154,155,156,157,158,159,160,161,162,163,164,165,166,167,168,169,170,171,172,173,174,175,176,177,178,179,180,181,182,183,184,185,186,187,188,189,190,191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,254,255,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,297,298,299,300,301,302,303,304,305,306,307,308,309,310,311,312,313,314,315,316,317,318,319,320,321,322,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354,355,356,357,358,359,360,361,362,363,364,365,366,367,368,369,370,371,372,373,374,375,376,377,378,379,380,381,382,383,384,385,386,387,388,389,390,391,392,393,394,395,396,397,398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415,416,417,418,419,420,421,422,423,424,425,426,427,428,429,430,431,432,433,434,435,436,437,438,439,440,441,442,443,444,445,446,447,448,449,450,451,452,453,454,455,456,457,458,459,460,461,462,463,464,465,466,467,468,469,470,471,472,473,474,475,476,477,478,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493,494,495,496,497,498,499,500,501,502,503,504,505,506,507,508,509,510,511,512,513,514,515,516,517,518,519,520,521,522,523,524,525,526,527,528,529,530,531,532,533,534,535,536,537,538,539,540,541,542,543,544,545,546,547,548,549,550,551,552,553,554,555,556,557,558,559,560,561,562,563,564,565,566,567,568,569,570,571,572,573,574,575,576,577,578,579,580,581,582,583,584,585,586,587,588,589,590,591,592,593,594,595,596,597,598,599,600,601,602,603,604,605,606,607,608,609,610,611,612,613,614,615,616,617,618,619,620,621,622,623,624,625,626,627,628,629,630,631,632,633,634,635,636,637,638,639,640,641,642,643,644,645,646,647,648,649,650,651,652,653,654,655,656,657,658,659,660,661,662,663,664,665,666,667,668,669,670,671,672,673,674,675,676,677,678,679,680,681,682,683,684,685,686,687,688,689,690,691,692,693,694,695,696,697,698,699,700,701,702,703,704,705,706,707,708,709,710,711,712,713,714,715,716,717,718,719,720,721,722,723,724,725,726,727,728,729,730,731,732,733,734,735,736,737,738,739,740,741,742,743,744,745,746,747,748,749,750,751,752,753,754,755,756,757,758,759,760,761,762,763,764,765,766,767,768,769,770,771,772,773,774,775,776,777,778,779,780,781,782,783,784,785,786,787,788,789,790,791,792,793,794,795,796,797,798,799,800,801,802,803,804,805,806,807,808,809,810,811,812,813,814,815,816,817,818,819,820,821,822,823,824,825,826,827,828,829,830,831,832,833,834,835,836,837,838,839,840,841,842,843,844,845,846,847,848,849,850,851,852,853,854,855,856,857,858,859,860,861,862,863,864,865,866,867,868,869,870,871,872,873,874,875,876,877,878,879,880,881,882,883,884,885,886,887,888,889,890,891,892,893,894,895,896,897,898,899,900,901,902,903,904,905,906,907,908,909,910,911,912,913,914,915,916,917,918,919,920,921,922,923,924,925,926,927,928,929,930,931,932,933,934,935,936,937,938,939,940,941,942,943,944,945,946,947,948,949,950,951,952,953,954,955,956,957,958,959,960,961,962,963,964,965,966,967,968,969,970,971,972,973,974,975,976,977,978,979,980,981,982,983,984,985,986,987,988,989,990,991,992,993,994,995,996,997,998,999,1000,1001];
        return canarybit[bitpos]
    except:
        raise_exception('[ERROR] Exception thrown CanaryMap() function')





'''
    ================================================================
                    Externally Called Functions
    ================================================================
'''


def init():
    '''
        ============================================================
        Placeholder External Function for Execution during Init Flow

        For example: Parsing Config file & storing in Dictionary !
        ============================================================

    :return: True/False
    '''

    # Global Variables ( i.e. Variables Outside this Instance !)
    global g_oIni  # Now the Ini File Object is Global, & it's data accessible to other functions outside of Init()

    # Local Variables
    b_result = True

    try:
        #print '[DEBUG] Calling init() Function'

        # If Ini File Specified in UF Arguments
        if 'INIFILE' in uf_args.keys():
            # Create Ini Object & Parse Sample Ini File
            g_oIni = ObjIni(uf_args['INIFILE'])
            g_oIni.parse_file()

            # Print Final set of Parameters ( For Debugging Final Structure )
            #if 'DEBUG' in uf_args.keys() and uf_args['DEBUG']:
                #print '[DEBUG] Ini Parameters: %s' % g_oIni.print_params()

        else:
            # Error in Function Call, no Ini File Specified
            raise_exception('[ERROR] No Ini File Specified in UF Arguments !')
            b_result = False

        # Check if Passing
        if b_result:
            update_exit_port(STD_PASS_PORT)
        else:
            update_exit_port(STD_FAIL_PORT)
        return True
    except Exception:
        raise_exception('[ERROR] Exception thrown during init() function')


def start_of_die():
    '''
        ============================================================
        Placeholder External Function for Execution during
        Start-Of-Die.

        For Example: Setting GSDS Values at the start of every die
        to default values.
        ============================================================

    :return: True/False
    '''

    # Local Variables
    b_result = True

    try:
        #print '[DEBUG] Calling start_of_die() Function'

        # Check if Passing
        if b_result:
            update_exit_port(STD_PASS_PORT)
        else:
            update_exit_port(STD_FAIL_PORT)
        return True
    except Exception:
        raise_exception('[ERROR] Exception thrown during start_of_die() function')



def end_of_die():
    '''
        ============================================================
        Placeholder External Function for Execution during
        End-Of-Die.

        For Example: Writing the current values of GSDS Tokens to
        the ITUFF
        ============================================================

    :return: True/False
    '''

    # Local Variables
    b_result = True

    try:
        #print '[DEBUG] Calling end_of_die() Function'

        # Check if Passing
        if b_result:
            update_exit_port(STD_PASS_PORT)
        else:
            update_exit_port(STD_FAIL_PORT)
        return True
    except Exception:
        raise_exception('[ERROR] Exception thrown during end_of_die() function')




def fuse_raster():
    '''
        ============================================================
        Sample Externally Called Function, Function must return a
        True/False so failures can be tracked for the ultimate
        result of the UF Call
        ============================================================

    :return: True/False
    '''

    # Local Variables
    b_result = True
    failbits_global = 'FUSEUFGL.UFGL_MISMATCH_BITS'
    fusereadstring_global = 'FUSEUFGL.UFGL_Fuse_Read_String'
    burnstring_global = 'FUSEUFGL.UFGL_Fuse_Burn_String'
    canary_expect_global = 'FUS_FUSEREAD.UFGL_Canary_Expect'
    chkb_gsds = 'FuseCheckerboardBurn'
    ichkb_gsds = 'FuseiCheckerboardBurn'
    ignore_bits = ["48352","48353","48354","48355","48356","48357","48358","48359","48360","48361","48362","48363","48364"]

    #ir_expect_global = 'FUS_FUSEREAD.UFGL_IR_Expect'
    #mode = 'RAP'

    try:
        #print '[DEBUG] Calling fuse_raster() Function'

        if 'ARRAY' and 'MODE' in uf_args.keys():
            s_array = uf_args['ARRAY']
            s_mode = uf_args['MODE']
        else:
            raise_exception('[ERROR] Exception thrown during fuse_raster() function, Missing UF Arguments!')

        ####Print Header####
        #print 'printing header'
        tRetVal = fuse_raster_tfile_header(s_array)
        if tRetVal != 1:
            #print 'failed fuse_raster_tfile_header function'
            return False

    #### Get values of input globals####
    #print 'getting fuse string data from global'
        try:
            fusereadstring_value = (evg.GetTpGlobalValue(fusereadstring_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value for fusestring\n")
        #print fusereadstring_value


    #### Get values of canary expect globals####
    #print 'getting canary expect data from global'
#        try:
#            canaryexpect_value = (evg.GetTpGlobalValue(canary_expect_global, "string"))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for canary expect\n")
        #print canaryexpect_value

    #### Get values of IR expect globals####
    #print 'getting IR expect data from global'
        #try:
        #    irexpect_value = (evg.GetTpGlobalValue(ir_expect_global, "string"))
        #except:
        #    raise StandardError("Global Error: Failed to get global Value for canary expect\n")
        #print irexpect_value


    #print 'getting failbit data from global'
        try:
            failbits_value = (evg.GetTpGlobalValue(failbits_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value\n")
        #print failbits_value


        try:
            burnstring_value = (evg.GetTpGlobalValue(burnstring_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value for burnstring\n")
        #print burnstring_value

#        try:
#            chkbstring_value = (evg.GetGSDSData("FuseCheckerboardBurn", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for chkb burnstring\n")
        #print burnstring_value

#        try:
#            ichkbstring_value = (evg.GetGSDSData("FuseiCheckerboardBurn", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for ichkb burnstring\n")
        #print burnstring_value

#        try:
#            yieldmonstring_value = (evg.GetGSDSData("FuseYieldmonCompare", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get GSDS Value for FuseYieldmonBurn\n")

        #if (s_mode == "YIELDMONIR" or s_mode == "YIELDMONRAP"):
        #    try:
        #        fusereadstring_value = (evg.GetGSDSData("FuseYieldmonRead", "string","UNT",-99,0))
        #    except:
        #        raise StandardError("Global Error: Failed to get GSDS Value for FuseYieldmonRead\n")

        #print fusereadstring_value
        fusestring_length = len(fusereadstring_value)

        if s_mode == "CANARY":
            expectstring = canaryexpect_value
        elif (s_mode == "IR" or s_mode == "YIELDMONIR"):
            expectstring = "0" * fusestring_length
        elif (s_mode == "RAPV" or s_mode=="RAPR"):
            expectstring = burnstring_value
        elif s_mode == "CHKB":
            expectstring = chkbstring_value
        elif s_mode == "ICHKB":
            expectstring = ichkbstring_value
        elif s_mode == "YIELDMONRAP":
            expectstring = burnstring_value
        else:
            raise StandardError("Invalid MODE specified. expecting CANARY, IR, RAPV, RAPR, CHKB or ICHKB\n")

        ####Split failbit string into list####
        #print 'splitting bit list'
        failbits_list = failbits_value.split(",")

        #print expectstring
    #print fusereadstring_value
        #print fusestring_length

        ####Loop for failbits logging
        #print 'printing fail bits'
        for bit in failbits_list:
            if (bit in ignore_bits):
                #print "skipping bit"
                #print bit
                continue
            bitpos = fusestring_length - 1 - int(bit)
            if s_mode == "CANARY":
                #rasterbit = CanaryMap(int(bit))
                #sToWrite = "cb" + str(rasterbit) + "," + expectstring[bitpos] + "," + fusereadstring_value[bitpos] + "\n"
                sToWrite = "cb" + bit + "," + expectstring[bitpos] + "," + fusereadstring_value[bitpos] + "\n"
            else:
                sToWrite = "db" + bit + "," + expectstring[bitpos] + "," + fusereadstring_value[bitpos] + "\n"
            #print sToWrite
            tRetVal = evg.TFileWrite(sToWrite)
            if tRetVal != 1:
                #print 'failed rfilewrite'
                return False
            #print bit
        # tRetVal = evg.TFileClose()
        # if tRetVal != 1:
        #     return False

        # Check if Passing
        if b_result:
            update_exit_port(STD_PASS_PORT)
        else:
            update_exit_port(STD_FAIL_PORT)
        return True
    except Exception:
        #print "Failed";
        raise_exception('[ERROR] Exception thrown during external_1() function')

def log_failing_bits():
    '''
        ============================================================
        Sample Externally Called Function, Function must return a
        True/False so failures can be tracked for the ultimate
        result of the UF Call
        ============================================================

    :return: True/False
    '''

    # Local Variables
    b_result = True
    failbits_global = 'FUSEUFGL.UFGL_MISMATCH_BITS'
    fusereadstring_global = 'FUSEUFGL.UFGL_Fuse_Read_String'
    burnstring_global = 'FUSEUFGL.UFGL_Fuse_Burn_String'
    canary_expect_global = 'FUS_FUSEREAD.UFGL_Canary_Expect'
    chkb_gsds = 'FuseCheckerboardBurn'
    ichkb_gsds = 'FuseiCheckerboardBurn'
    ignore_bits = ["48352","48353","48354","48355","48356","48357","48358","48359","48360","48361","48362","48363","48364"]
    #ir_expect_global = 'FUS_FUSEREAD.UFGL_IR_Expect'
    #mode = 'RAP'
    sTestName = evg.GetInstanceName()

    try:
        #print '[DEBUG] Calling log_failing_bits() Function - EMS'

        if 'MODE' in uf_args.keys():
            s_mode = uf_args['MODE']
        else:
            raise_exception('[ERROR] Exception thrown during Log_failing_bita() function, Missing UF Arguments!')

        #print 'COMMENT: Before getting globals and GSDS'

        #### Get values of input globals####
        #print 'getting fuse string data from global'
        try:
            fusereadstring_value = (evg.GetTpGlobalValue(fusereadstring_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value for fusestring\n")
        #print 'COMMENT: Read fusereadstring_global'


        #### Get values of canary expect globals####
        #print 'COMMENT: getting canary expect data from global'
#        try:
#            canaryexpect_value = (evg.GetTpGlobalValue(canary_expect_global, "string"))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for canary expect\n")
        #print canaryexpect_value

        #### Get values of IR expect globals####
        #print 'getting IR expect data from global'
        #try:
        #    irexpect_value = (evg.GetTpGlobalValue(ir_expect_global, "string"))
        #except:
        #    raise StandardError("Global Error: Failed to get global Value for canary expect\n")
        #print irexpect_value


        #print 'COMMENT: getting failbit data from global'
        try:
            failbits_value = (evg.GetTpGlobalValue(failbits_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value\n")
        #print failbits_value


        try:
            burnstring_value = (evg.GetTpGlobalValue(burnstring_global, "string"))
        except:
            raise StandardError("Global Error: Failed to get global Value for burnstring\n")
        #print 'COMMENT: Read burnstring global'

#        try:
#            chkbstring_value = (evg.GetGSDSData("FuseCheckerboardBurn", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for burnstring\n")
        #print 'COMMENT: Read FuseCheckerboardBurn GSDS'

#        try:
#            ichkbstring_value = (evg.GetGSDSData("FuseiCheckerboardBurn", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get global Value for burnstring\n")
        #print 'COMMENT: Read FuseiCheckerboardBurn GSDS'

        #sVal = evg. GetGSDSData("ABint","integer","LOT",-99,0)

#        try:
#            yieldmonstring_value = (evg.GetGSDSData("FuseYieldmonCompare", "string","UNT",-99,0))
#        except:
#            raise StandardError("Global Error: Failed to get GSDS Value for FuseYieldmonBurn\n")

        #if (s_mode == "YIELDMONIR" or s_mode == "YIELDMONRAP"):
        #    try:
        #        fusereadstring_value = (evg.GetGSDSData("FuseYieldmonRead", "string","UNT",-99,0))
        #    except:
        #        raise StandardError("Global Error: Failed to get GSDS Value for FuseYieldmonRead\n")

        #print 'COMMENT: Read all GSDS and Uservar'

        fusestring_length = len(fusereadstring_value)


        if s_mode == "CANARY":
            expectstring = canaryexpect_value
        elif (s_mode == "IR" or s_mode == "YIELDMONIR"):
            expectstring = "0" * fusestring_length
        elif (s_mode == "RAPV" or s_mode=="RAPR"):
            expectstring = burnstring_value
        elif s_mode == "CHKB":
            expectstring = chkbstring_value
        elif s_mode == "ICHKB":
            expectstring = ichkbstring_value
        elif s_mode == "YIELDMONRAP":
            expectstring = burnstring_value
        else:
            raise StandardError("Invalid MODE specified. expecting CANARY, IR, RAPV, RAPR, YIELDMONIR, YIELDMONRAP, CHKB or ICHKB\n")

        ####Split failbit string into list####
        #print 'splitting bit list'
        failbits_list = failbits_value.split(",")

        e0r1 = 0
        e0ru = 0
        e1r0 = 0
        e1ru = 0

        #print expectstring
        #print fusereadstring_value
        #print fusestring_length

        ####Loop for failbits logging
        #print 'printing fail bits'
        for bit in failbits_list:
            if (bit in ignore_bits):
                continue
            bitpos = fusestring_length - 1 - int(bit)

            if expectstring[bitpos] == "0":
                if fusereadstring_value[bitpos] == "1":
                    e0r1 = e0r1 + 1
                if fusereadstring_value[bitpos] == "u" or fusereadstring_value[bitpos] == "U":
                    e0ru = e0ru + 1
            if expectstring[bitpos] == "1":
                if fusereadstring_value[bitpos] == "0":
                    e1r0 = e1r0 + 1
                if fusereadstring_value[bitpos] == "u" or fusereadstring_value[bitpos] == "U":
                    e1ru = e1ru + 1
        #print bit
        #print "before calcs"
        e0fail = e0r1 + e0ru
        e1fail = e1r0 + e1ru
        #print "before ituff print"
        sToWrite = "2_tname_%s_BITFAIL_E0FAIL\n2_mrslt_%d\n" % (sTestName, e0fail)
        evg.PrintToItuff(sToWrite)
        sToWrite = "2_tname_%s_BITFAIL_E0R1\n2_mrslt_%d\n" % (sTestName, e0r1)
        evg.PrintToItuff(sToWrite)
        sToWrite = "2_tname_%s_BITFAIL_E0RU\n2_mrslt_%d\n" % (sTestName, e0ru)
        evg.PrintToItuff(sToWrite)
        sToWrite = "2_tname_%s_BITFAIL_E1FAIL\n2_mrslt_%d\n" % (sTestName, e1fail)
        evg.PrintToItuff(sToWrite)
        sToWrite = "2_tname_%s_BITFAIL_E1R0\n2_mrslt_%d\n" % (sTestName, e1r0)
        evg.PrintToItuff(sToWrite)
        sToWrite = "2_tname_%s_BITFAIL_E1RU\n2_mrslt_%d\n" % (sTestName, e1ru)
        evg.PrintToItuff(sToWrite)


        # Check if Passing
        if b_result:
            update_exit_port(STD_PASS_PORT)
        else:
            update_exit_port(STD_FAIL_PORT)
        return True
    except Exception:
        #print "Failed"
        raise_exception('[ERROR] Exception thrown during log_failing_bits() function')



'''
    Check to See it Using EVG Simulator, or on a Tester
'''
if 'SetupEVGEnv' in dir(evg):
    # Simulator

    # Add EVG Config Setup Command Here !
    # e.g. evg.SetupEVGEnv('validation.cfg')
    evg.SetupEVGEnv('validation.cfg')

    ''' Call Main Function '''
    main()
else:
    # Tester

    ''' Call Main Function '''
    main()

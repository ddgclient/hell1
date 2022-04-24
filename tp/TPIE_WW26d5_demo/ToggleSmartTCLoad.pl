#!/usr/local/bin/perl

################################################################
#	Purpose: 	This script allows the user to toggle the value of SmartTC to ON or OFF
#  				It goes through all tpl files and changes for any instance iCBkgndTest test class
#	Author:		Samira Ejaz
################################################################

use strict; 
use warnings;
use File::Copy qw(move);
use File::Copy qw(copy);

# check if we should fix of undo previous fix
my $runUndo = 0;
my $printToConsole = 1;
my $smart_tc = "UNDEFINED";

	
if(defined $ARGV[0]){
	# if($ARGV[0] =~ /undo/i){
		# $runUndo = 1;
	# }elsif($ARGV[0] =~ /off/i){
		# $printToConsole = 0;
	# }
	
	if($ARGV[0] =~ /ON/i){
		$smart_tc = "ON";			# by default it is ON		
	}elsif($ARGV[0] =~ /OFF/i){
		$smart_tc = "OFF";
	}else{
		die "\n\t**** Usage: perl ToggleSmartTCLoad.pl (ON|OFF) ****\n";
	}
}else{
	die "\n\t**** Usage: perl ToggleSmartTCLoad.pl (ON|OFF) ****\n";
}
# print note to user
print "PLEASE DO NOT EXIT IN THE MIDDLE TO AVOID TPL CORRUPTION!!!\n";

# get timestamp
my $timestamp = getTimestamp();

# run the script
my $modularTPL_ref = parse_stpl();
foreach my $tpl (sort @{$modularTPL_ref}){
	if($runUndo){
		# undoRun($tpl);
		print "Undo disabled\n";
	}else{
		my $result = UpdateTPL($tpl);		#add .tpl extension to the folder name to get the file name		
		if($result){
			exit(1);
		}
	}
}
print "Finished changing smart_TC_load to $smart_tc\n\n";

###################################################
# go through all the tpl files and get set value for any
# that has the smart TC capability
###################################################
sub UpdateTPL{
    my $origFile = shift;		 	   
	my $tempFile = $origFile."_temp.txt";
	my $fileHadTest = 0;
	
	# open files for reading and writing
	open (TPLFILE_READ, "$origFile") or die ("Cannot open file $origFile Error: $!");
    open (TPLFILE_WRITE, ">$tempFile") or die ("Cannot open file $tempFile Error: $!");

    while (<TPLFILE_READ>)
    {
		print TPLFILE_WRITE "$_";

		if (/^Test\s+(\w*)\s+(\w*)$/) {
            my $test_class = $1;
            my $test_name = $2;       		
					
            # Parse the entire Test record
            $_ = <TPLFILE_READ>;
            chomp;

            while (! m/}/) {			#parse until you get the } which signals that you have reached the end of the test record
				my($parameter, $value) = ("", "");
                if (/\s*\t*(\w+)\s+=\s+(.+);/) {
                    $parameter = $1;
                    $value = $2;        
                    $value =~ s/"//g;	        # Remove double quotes
                }
				
				# change if match else print out original
				if($test_class =~ /iCBkgndTest/ and $parameter =~ /smart_TC_load/){
					$fileHadTest = 1;
					print TPLFILE_WRITE "\t$parameter = \"$smart_tc\";\n";						
				}else{
					print TPLFILE_WRITE "$_\n";
				}
                
				$_ = <TPLFILE_READ>;
                chomp;
           }
		   print TPLFILE_WRITE "$_\n";		# print the last ending curly brace }
        }
    }
    close (TPLFILE_READ);
    close (TPLFILE_WRITE);
	
	# only update for files that contain a smart tc instance
	if($fileHadTest == 1){
		# get name of orig file to rename the temp file
		my $newFile = $origFile;
		
		# rename old file	
		my $oldFile = $origFile;
		$oldFile =~ s/\.tpl/_SmartTCtoggle_$timestamp\.tpl/;
		# $oldFile =~ s/\.tpl/_SmartTCtoggle\.tpl/;

		print "original = $oldFile\n" if($printToConsole);	
		move $origFile, $oldFile;		
		move $tempFile, $newFile;
	}else
	{
		# remove the temp file
		unlink $tempFile;
	}
	
	return 0;
}


###################################################
# replace new file with original file
###################################################
sub undoRun{	
	my $file = shift;		 	   	
	my $oldFile = $file;
	$oldFile =~ s/\.tpl/_SmartTCtoggle\.tpl/;	
	move $oldFile, $file;
	print "reverting to $file\n" if($printToConsole);
	# move $file, $oldFile;	
}

###################################################
# get current timestamp
###################################################
sub getTimestamp{
	my @time = localtime();
	$time[4] += 1;
	$time[5] += 1900;
	return "$time[5].$time[4].$time[3].$time[2].$time[1].$time[0]";
}

########################################################################
# get name of stpl file to parse
########################################################################
sub getSTPLName{
	my $directory = ".";		# current directory
	my @stpls;
	my $testProgName = "UNDEFINED";	
	opendir (DIR, $directory) or die "Could not open directory $directory. Error: $!\n";
	while(my $file = readdir(DIR)){	#
		if($file =~ /(.*\.stpl)$/){
			push @stpls, $1;
		}
	}
	
	if(scalar @stpls == 1){
		return $stpls[0];
	}else{
		return choose_stpl(\@stpls);
	}		
}

########################################################################
# have user select stpl
########################################################################
sub choose_stpl
{
	my $stpls = shift;
	my @stpls = @{$stpls};

	# print program options
	print "\nChoose program type (default = $stpls[0])\n";
	for my $i (0 .. $#stpls){
		print "\t$i = $stpls[$i]\n";
	}
	
	# get user input, check if valid
	my $index = <STDIN>;
	chomp $index ;
	if (0 > $index || $#stpls < $index ){	# reset to 2P2 if error
		print "Could not interpret input '$index', using default value '$stpls[0]'\n";
		$index = 0;
	}
	
	# determine stpl file
	my $stpl = $stpls[$index];
	return $stpl;
}

########################################################################
# parse a given stpl file
########################################################################
sub parse_stpl{
	my @files;
	my $stpl = getSTPLName();
	open STPL, $stpl or die "Could not open stpl file '$stpl' file\n";
	while(my $file = <STPL>){
		if($file =~ /(Module.*);/){		# skip final module and remove semicolens
			push @files, $1;
		}
	}
	return \@files;
}
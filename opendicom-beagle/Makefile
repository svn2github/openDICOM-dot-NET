#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$


IGNORE_WARNINGS=219


SOURCES=$(shell find src -name *.cs)
BACKUPS=$(shell find -name *~)

DOCS=README INSTALL GPL license copyright release changelog

ELEMENT_DIC=dicom-elements-2004.dic
UID_DIC=dicom-uids-2004.dic
GROUP=opendicom.net
NAME=opendicom-beagle
DLL_NAME=FilterDicom.dll

DICOM_BEAGLE_COMPILE=gmcs -target:library\
	-nowarn:$(IGNORE_WARNINGS)\
	-out:bin/$(DLL_NAME)\
	-r:/usr/lib/beagle/Util.dll\
	-r:/usr/lib/beagle/Beagle.dll\
	-r:/usr/lib/beagle/BeagleDaemonPlugins.dll\
	-pkg:opendicom-sharp\
	$(SOURCES)

all: build 

build: 
	@sh check.sh --cmd -e gmcs
	@sh check.sh --lib -e opendicom-sharp
	@sh output.sh --cyan -n "echo $(DICOM_BEAGLE_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_BEAGLE_COMPILE)"
	@chmod -x bin/$(DLL_NAME)

clean:
	@rm -Rf bin/$(DLL_NAME) $(BACKUPS)

install:
	@sh check.sh --dir -e /usr/lib/beagle/Filters
	@sh check.sh --lib -e opendicom-sharp
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp bin/$(DLL_NAME) /usr/share/$(GROUP)/$(NAME)/
	@ln -s /usr/share/$(GROUP)/$(NAME)/$(DLL_NAME) /usr/lib/beagle/Filters/$(DLL_NAME)
	@ln -s /usr/share/$(GROUP)/opendicom-sharp/opendicom-sharp.dll /usr/lib/beagle/Filters/opendicom-sharp.dll
	@cp dd/$(ELEMENT_DIC) dd/$(UID_DIC) /usr/share/$(GROUP)/$(NAME)/dd/
	@chmod 644 /usr/share/$(GROUP)/$(NAME)/$(DLL_NAME)

uninstall:
	@rm -Rf /usr/share/$(GROUP)/$(NAME)
	@rm -Rf /usr/share/doc/$(GROUP)/$(NAME)
	@rmdir --ignore-fail-on-non-empty /usr/share/$(GROUP)
	@rmdir --ignore-fail-on-non-empty /usr/share/doc/$(GROUP)
	@rm -f /usr/lib/beagle/Filters/$(DLL_NAME)
	@rm -f /usr/lib/beagle/Filters/opendicom-sharp.dll

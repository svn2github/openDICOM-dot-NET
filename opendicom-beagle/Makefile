#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$


BEAGLE_HOME=/usr/lib/beagle


IGNORE_WARNINGS=219


SOURCES=$(shell find src -name *.cs)
BACKUPS=$(shell find -name *~)

DOCS=README INSTALL GPL license copyright release changelog

OUTPUT_DIRS=bin

ELEMENT_DIC=dicom-elements-2007.dic
UID_DIC=dicom-uids-2007.dic
LIB=opendicom-sharp
LIB_VERSION=0.1.1
GROUP=opendicom.net
NAME=opendicom-beagle
DLL_NAME=FilterDicom.dll

DICOM_BEAGLE_COMPILE=gmcs -target:library\
	-nowarn:$(IGNORE_WARNINGS)\
	-out:bin/$(DLL_NAME)\
	-r:$(BEAGLE_HOME)/Util.dll\
	-r:$(BEAGLE_HOME)/Beagle.dll\
	-r:$(BEAGLE_HOME)/BeagleDaemonPlugins.dll\
	-pkg:opendicom-sharp\
	$(SOURCES)

all: build 

build: 
	@sh check.sh --cmd -e gmcs
	@sh check.sh --lib -e $(LIB)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@for DIR in $(OUTPUT_DIRS); do mkdir -p $$DIR; done
	@sh output.sh --cyan -n "echo $(DICOM_BEAGLE_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_BEAGLE_COMPILE)"
	@chmod -x bin/$(DLL_NAME)

clean:
	@rm -Rf $(OUTPUT_DIRS) $(BACKUPS)

install:
	@sh check.sh --dir -e $(BEAGLE_HOME)/Filters
	@sh check.sh --lib -e $(LIB)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp bin/$(DLL_NAME) /usr/share/$(GROUP)/$(NAME)/
	@ln -s /usr/share/$(GROUP)/$(NAME)/$(DLL_NAME) $(BEAGLE_HOME)/Filters/$(DLL_NAME)
	@ln -s /usr/share/$(GROUP)/opendicom-sharp/opendicom-sharp.dll $(BEAGLE_HOME)/Filters/opendicom-sharp.dll
	@cp dd/$(ELEMENT_DIC) dd/$(UID_DIC) /usr/share/$(GROUP)/$(NAME)/dd/
	@chmod 644 /usr/share/$(GROUP)/$(NAME)/$(DLL_NAME)

uninstall:
	@rm -Rf /usr/share/$(GROUP)/$(NAME)
	@rm -Rf /usr/share/doc/$(GROUP)/$(NAME)
	@rmdir --ignore-fail-on-non-empty /usr/share/$(GROUP)
	@rmdir --ignore-fail-on-non-empty /usr/share/doc/$(GROUP)
	@rm -f $(BEAGLE_HOME)/Filters/$(DLL_NAME)
	@rm -f $(BEAGLE_HOME)/Filters/opendicom-sharp.dll

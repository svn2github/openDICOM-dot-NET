#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$


IGNORE_WARNINGS=162,169,168


DICOM_FILE_NAVI=dicom-file-navigator

DICOM_FILE_NAVI_SOURCES=$(shell find src/$(DICOM_FILE_NAVI) -name *.cs)

BACKUPS=$(shell find -name *~)

DOCS=README INSTALL GPL license copyright release changelog

OUTPUT_DIRS=bin doc

ELEMENT_DIC=dicom-elements-2007.dic
UID_DIC=dicom-uids-2007.dic
LIB=opendicom-sharp
LIB_VERSION=0.1.1
NAME=opendicom-navigator
GROUP=opendicom.net

GTK_SHARP=gtk-sharp-2.0
GTK_VERSION=2.10
GLADE_SHARP=glade-sharp-2.0
GLADE_VERSION=2.10


DICOM_FILE_NAVI_COMPILE=gmcs -target:exe\
	-nowarn:$(IGNORE_WARNINGS)\
	-pkg:$(LIB)\
	-pkg:$(GLADE_SHARP)\
	-pkg:$(GTK_SHARP)\
	-resource:res/$(DICOM_FILE_NAVI).glade\
	-resource:res/sextant.png\
	-resource:res/sextant.ico\
	-resource:res/about.png\
	-resource:res/brighten.png\
	-resource:res/darken.png\
	-resource:res/gimp.png\
	-resource:conf/$(DICOM_FILE_NAVI).config\
	-doc:doc/$(DICOM_FILE_NAVI).xml\
	-out:bin/$(DICOM_FILE_NAVI).exe\
	$(DICOM_FILE_NAVI_SOURCES)


all: build

build:
	@sh check.sh --cmd -e gmcs
	@sh check.sh --lib -e $(LIB) $(GTK_SHARP) $(GLADE_SHARP)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@sh check.sh --lib-version -ge $(GTK_VERSION) $(GTK_SHARP)
	@sh check.sh --lib-version -ge $(GLADE_VERSION) $(GLADE_SHARP)
	@for DIR in $(OUTPUT_DIRS); do mkdir -p $$DIR; done
	@sh output.sh --cyan -n "echo $(DICOM_FILE_NAVI_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_FILE_NAVI_COMPILE)"

clean:
	@rm -Rf $(OUTPUT_DIRS) res/*.bak $(BACKUPS)

install:
	@sh check.sh --cmd -e mono
	@sh check.sh --lib -e $(LIB) $(GTK_SHARP) $(GLADE_SHARP)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@sh check.sh --lib-version -ge $(GTK_VERSION) $(GTK_SHARP)
	@sh check.sh --lib-version -ge $(GLADE_VERSION) $(GLADE_SHARP)
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir /usr/share/$(GROUP)/$(NAME)/bin
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp res/$(DICOM_FILE_NAVI).desktop /usr/share/applications/
	@cp res/sextant.png /usr/share/pixmaps/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp dd/$(ELEMENT_DIC) dd/$(UID_DIC) /usr/share/$(GROUP)/$(NAME)/dd/
	@cp bin/*.exe /usr/share/$(GROUP)/$(NAME)/bin/
	@echo "#!/bin/sh" > /usr/bin/$(DICOM_FILE_NAVI)
	@echo "export MONO_PATH=/usr/lib/mono/$(LIB)" >> /usr/bin/$(DICOM_FILE_NAVI)
	@echo "mono /usr/share/$(GROUP)/$(NAME)/bin/$(DICOM_FILE_NAVI).exe \$$@" >> /usr/bin/$(DICOM_FILE_NAVI)
	@chmod 755 /usr/bin/$(DICOM_FILE_NAVI)

uninstall:
	@rm -f /usr/share/applications/$(DICOM_FILE_NAVI).desktop
	@rm -f /usr/share/pixmaps/sextant.png
	@rm -Rf /usr/share/$(GROUP)/$(NAME)
	@rm -Rf /usr/share/doc/$(GROUP)/$(NAME)
	@rmdir --ignore-fail-on-non-empty /usr/share/$(GROUP)
	@rmdir --ignore-fail-on-non-empty /usr/share/doc/$(GROUP)
	@rm -f /usr/bin/$(DICOM_FILE_NAVI)

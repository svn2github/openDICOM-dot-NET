#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$


IGNORE_WARNINGS=162,168


SOURCES=$(shell find src -name *.cs)
BACKUPS=$(shell find -name *~)

DOCS=README INSTALL LGPL license copyright release changelog

OUTPUT_DIRS=bin doc

ELEMENT_DIC=dicom-elements-2007.dic
UID_DIC=dicom-uids-2007.dic
GROUP=opendicom.net
NAME=opendicom-sharp

DICOM_SHARP_COMPILE=mcs -target:library\
	-nowarn:$(IGNORE_WARNINGS)\
	-doc:doc/$(NAME).xml\
	-out:bin/$(NAME).dll\
	$(SOURCES)

DICOM_SHARP_MONODOC=monodocer -assembly:bin/$(NAME).dll -path:doc -updateto:doc/monodoc


all: build 

build: 
	@sh check.sh --cmd -e mcs
	@for DIR in $(OUTPUT_DIRS); do mkdir -p $$DIR; done
	@sh output.sh --cyan -n "echo $(DICOM_SHARP_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_SHARP_COMPILE)"
	@chmod -x bin/$(NAME).dll

clean:
	@rm -Rf $(OUTPUT_DIRS) $(BACKUPS)

monodoc:
	@sh check.sh --cmd -e monodocer
	@sh output.sh --cyan -n "echo $(DICOM_SHARP_MONODOC)"
	@sh output.sh --brown -n "$(DICOM_SHARP_MONODOC)"

install:
	@sh check.sh --cmd -e pkg-config
	@sh check.sh --dir -e /usr/lib/mono /usr/lib/pkgconfig
	@mkdir /usr/lib/mono/$(NAME)
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp bin/$(NAME).dll /usr/share/$(GROUP)/$(NAME)/
	@ln -s /usr/share/$(GROUP)/$(NAME)/$(NAME).dll /usr/lib/mono/$(NAME)/$(NAME).dll
	@cp dd/$(ELEMENT_DIC) dd/$(UID_DIC) /usr/share/$(GROUP)/$(NAME)/dd/
	@cp pkgconfig/$(NAME).pc /usr/lib/pkgconfig/
	@chmod 644 /usr/share/$(GROUP)/$(NAME)/$(NAME).dll
	@chmod 644 /usr/lib/pkgconfig/$(NAME).pc

uninstall:
	@rm -Rf /usr/lib/mono/$(NAME)
	@rm -Rf /usr/share/$(GROUP)/$(NAME)
	@rm -Rf /usr/share/doc/$(GROUP)/$(NAME)
	@rmdir --ignore-fail-on-non-empty /usr/share/$(GROUP)
	@rmdir --ignore-fail-on-non-empty /usr/share/doc/$(GROUP)
	@rm -f /usr/lib/pkgconfig/$(NAME).pc

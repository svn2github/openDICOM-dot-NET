#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$


IGNORE_WARNINGS=162


DICOM_FILE_TRANSFORM=dicom-file-transform
DICOM_FILE_QUERY=dicom-file-query
DICOM_DD_TRANSFORM=dicom-dd-transform
DICOM_DD_QUERY=dicom-dd-query

DICOM_FILE_TRANSFORM_SOURCES=$(shell find src/$(DICOM_FILE_TRANSFORM) -name *.cs)
DICOM_FILE_QUERY_SOURCES=$(shell find src/$(DICOM_FILE_QUERY) -name *.cs)
DICOM_DD_TRANSFORM_SOURCES=$(shell find src/$(DICOM_DD_TRANSFORM) -name *.cs)
DICOM_DD_QUERY_SOURCES=$(shell find src/$(DICOM_DD_QUERY) -name *.cs)

BACKUPS=$(shell find -name *~)

DOCS=README INSTALL GPL license copyright release changelog


ELEMENT_DIC=dicom-elements-2007.dic
UID_DIC=dicom-uids-2007.dic
LIB=opendicom-sharp
LIB_VERSION=0.1.1
NAME=opendicom-utils
GROUP=opendicom.net


DICOM_FILE_TRANSFORM_COMPILE=mcs -target:exe\
	-nowarn:$(IGNORE_WARNINGS)\
	-pkg:$(LIB)\
	-doc:doc/$(DICOM_FILE_TRANSFORM).xml\
	-out:bin/$(DICOM_FILE_TRANSFORM).exe\
	 $(DICOM_FILE_TRANSFORM_SOURCES)

DICOM_FILE_QUERY_COMPILE=mcs -target:exe\
        -nowarn:$(IGNORE_WARNINGS)\
        -pkg:$(LIB)\
        -doc:doc/$(DICOM_FILE_QUERY).xml\
        -out:bin/$(DICOM_FILE_QUERY).exe\
         $(DICOM_FILE_QUERY_SOURCES)

DICOM_DD_TRANSFORM_COMPILE=mcs -target:exe\
        -nowarn:$(IGNORE_WARNINGS)\
        -pkg:$(LIB)\
        -doc:doc/$(DICOM_DD_TRANSFORM).xml\
        -out:bin/$(DICOM_DD_TRANSFORM).exe\
         $(DICOM_DD_TRANSFORM_SOURCES)

DICOM_DD_QUERY_COMPILE=mcs -target:exe\
        -nowarn:$(IGNORE_WARNINGS)\
        -pkg:$(LIB)\
        -doc:doc/$(DICOM_DD_QUERY).xml\
        -out:bin/$(DICOM_DD_QUERY).exe\
         $(DICOM_DD_QUERY_SOURCES)


all: build

build:
	@sh check.sh --cmd -e mcs
	@sh check.sh --lib -e $(LIB)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@sh output.sh --cyan -n "echo $(DICOM_DD_TRANSFORM_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_DD_TRANSFORM_COMPILE)"
	@sh output.sh --cyan -n "echo $(DICOM_DD_QUERY_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_DD_QUERY_COMPILE)"
	@sh output.sh --cyan -n "echo $(DICOM_FILE_QUERY_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_FILE_QUERY_COMPILE)"
	@sh output.sh --cyan -n "echo $(DICOM_FILE_TRANSFORM_COMPILE)"
	@sh output.sh --brown -n "$(DICOM_FILE_TRANSFORM_COMPILE)"

clean:
	@rm -f bin/*.exe doc/*.xml $(BACKUPS)

install:
	@sh check.sh --cmd -e mono
	@sh check.sh --lib -e $(LIB)
	@sh check.sh --lib-version -ge $(LIB_VERSION) $(LIB)
	@sh check.sh --dir -e /usr/lib/mono /usr/lib/pkgconfig
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir /usr/share/$(GROUP)/$(NAME)/bin
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp dd/$(ELEMENT_DIC) dd/$(UID_DIC) /usr/share/$(GROUP)/$(NAME)/dd/
	@cp bin/*.exe /usr/share/$(GROUP)/$(NAME)/bin/
	@echo "#!/bin/sh" > /usr/bin/$(DICOM_FILE_TRANSFORM)
	@echo "export MONO_PATH=/usr/lib/mono/$(LIB)" >> /usr/bin/$(DICOM_FILE_TRANSFORM)
	@echo "mono /usr/share/$(GROUP)/$(NAME)/bin/$(DICOM_FILE_TRANSFORM).exe \$$@" >> /usr/bin/$(DICOM_FILE_TRANSFORM)
	@chmod 755 /usr/bin/$(DICOM_FILE_TRANSFORM)
	@echo "#!/bin/sh" > /usr/bin/$(DICOM_FILE_QUERY)
	@echo "export MONO_PATH=/usr/lib/mono/$(LIB)" >> /usr/bin/$(DICOM_FILE_QUERY)
	@echo "mono /usr/share/$(GROUP)/$(NAME)/bin/$(DICOM_FILE_QUERY).exe \$$@" >> /usr/bin/$(DICOM_FILE_QUERY)
	@chmod 755 /usr/bin/$(DICOM_FILE_QUERY)
	@echo "#!/bin/sh" > /usr/bin/$(DICOM_DD_TRANSFORM)
	@echo "export MONO_PATH=/usr/lib/mono/$(LIB)" >> /usr/bin/$(DICOM_DD_TRANSFORM)
	@echo "mono /usr/share/$(GROUP)/$(NAME)/bin/$(DICOM_DD_TRANSFORM).exe \$$@" >> /usr/bin/$(DICOM_DD_TRANSFORM)
	@chmod 755 /usr/bin/$(DICOM_DD_TRANSFORM)
	@echo "#!/bin/sh" > /usr/bin/$(DICOM_DD_QUERY)
	@echo "export MONO_PATH=/usr/lib/mono/$(LIB)" >> /usr/bin/$(DICOM_DD_QUERY)
	@echo "mono /usr/share/$(GROUP)/$(NAME)/bin/$(DICOM_DD_QUERY).exe \$$@" >> /usr/bin/$(DICOM_DD_QUERY)
	@chmod 755 /usr/bin/$(DICOM_DD_QUERY)

uninstall:
	@rm -Rf /usr/share/$(GROUP)/$(NAME)
	@rm -Rf /usr/share/doc/$(GROUP)/$(NAME)
	@rmdir --ignore-fail-on-non-empty /usr/share/$(GROUP)
	@rmdir --ignore-fail-on-non-empty /usr/share/doc/$(GROUP)
	@rm -f /usr/bin/$(DICOM_FILE_TRANSFORM) /usr/bin/$(DICOM_DD_TRANSFORM) /usr/bin/$(DICOM_DD_QUERY) /usr/bin/$(DICOM_FILE_QUERY)

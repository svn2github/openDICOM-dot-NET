#!/usr/bin/make
# $Id $

GROUP=opendicom.net
NAME=Gobosh.Dicom

DIC=dd-ps3-2006.xml

SOURCES=$(shell find src -name *.cs)
BACKUPS=$(shell find -name *~)

DOCS=changelog copyright GPL INSTALL license release

OUTPUT_DIRS=bin doc

DICOM_SHARP_COMPILE=mcs -target:library\
	-doc:doc/$(NAME).xml\
	-out:bin/$(NAME).dll\
	$(SOURCES)

DICOM_SHARP_MONODOC=monodocer -assembly:bin/$(NAME).dll -path:doc -updateto:doc/monodoc


all: build 

build: 
	@for DIR in $(OUTPUT_DIRS); do mkdir -p $$DIR; done
	@$(DICOM_SHARP_COMPILE)
	@chmod -x bin/$(NAME).dll

clean:
	@rm -Rf $(OUTPUT_DIRS) $(BACKUPS)

monodoc:
	@$(DICOM_SHARP_MONODOC)

install:
	@mkdir /usr/lib/mono/$(NAME)
	@mkdir -p /usr/share/$(GROUP)/$(NAME)/dd
	@mkdir -p /usr/share/doc/$(GROUP)/$(NAME)/
	@cp $(DOCS) /usr/share/doc/$(GROUP)/$(NAME)/
	@cp bin/$(NAME).dll /usr/share/$(GROUP)/$(NAME)/
	@ln -s /usr/share/$(GROUP)/$(NAME)/$(NAME).dll /usr/lib/mono/$(NAME)/$(NAME).dll
	@cp dd/$(DIC) /usr/share/$(GROUP)/$(NAME)/dd/
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


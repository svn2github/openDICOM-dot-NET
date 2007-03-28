#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE=opendicom-beagle
LIB=opendicom-sharp
DOC=opendicom-sharp-doc
UTILS=opendicom-utils
NAVI=opendicom-navigator

BEAGLE_RELEASE=$(shell cat $(BEAGLE)/release)
LIB_RELEASE=$(shell cat $(LIB)/release)
DOC_RELEASE=$(shell cat $(DOC)/release)
UTILS_RELEASE=$(shell cat $(UTILS)/release)
NAVI_RELEASE=$(shell cat $(NAVI)/release)


all: build

clean: clean-packages
	@make -C $(BEAGLE) clean
	@make -C $(LIB) clean
	@make -C $(UTILS) clean
	@make -C $(NAVI) clean

build:
	@make -C $(BEAGLE) build
	@make -C $(LIB) build
	@make -C $(UTILS) build
	@make -C $(NAVI) build

install:
	@make -C $(BEAGLE) install
	@make -C $(LIB) install
	@make -C $(UTILS) install
	@make -C $(NAVI) install

uninstall:
	@make -C $(BEAGLE) uninstall
	@make -C $(LIB) uninstall
	@make -C $(UTILS) uninstall
	@make -C $(NAVI) uninstall

clean-packages:
	@rm -f *.tar.gz *.zip *.deb

md5sum:
	@sh checksum.sh beagle
	@sh checksum.sh lib
	@sh checksum.sh doc
	@sh checksum.sh utils
	@sh checksum.sh navi

debs: md5sum
	@dpkg -b $(BEAGLE)_deb $(BEAGLE)_$(BEAGLE_RELEASE)_all.deb
	@dpkg -b $(LIB)_deb $(LIB)_$(LIB_RELEASE)_all.deb
	@dpkg -b $(DOC)_deb $(DOC)_$(DOC_RELEASE)_all.deb
	@dpkg -b $(UTILS)_deb $(UTILS)_$(UTILS_RELEASE)_all.deb
	@dpkg -b $(NAVI)_deb $(NAVI)_$(NAVI_RELEASE)_all.deb	

tar-balls:
	@tar cvzf $(BEAGLE)_$(BEAGLE_RELEASE).tar.gz $(BEAGLE) > /dev/null
	@tar cvzf $(LIB)_$(LIB_RELEASE).tar.gz $(LIB) > /dev/null
	@tar cvzf $(UTILS)_$(UTILS_RELEASE).tar.gz $(UTILS) > /dev/null
	@tar cvzf $(NAVI)_$(NAVI_RELEASE).tar.gz $(NAVI) > /dev/null
	@tar cvzf $(DOC)_$(DOC_RELEASE).tar.gz $(DOC) > /dev/null
zips:
	@sh check.sh --cmd -e zip
	@zip -q -r $(BEAGLE)_$(BEAGLE_RELEASE).zip $(BEAGLE)
	@zip -q -r $(LIB)_$(LIB_RELEASE).zip $(LIB)
	@zip -q -r $(UTILS)_$(UTILS_RELEASE).zip $(UTILS)
	@zip -q -r $(NAVI)_$(NAVI_RELEASE).zip $(NAVI)
	@zip -q -r $(DOC)_$(DOC_RELEASE).zip $(DOC)

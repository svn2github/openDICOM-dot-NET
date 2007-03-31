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

debs:
	@bash build-deb.sh beagle
	@bash build-deb.sh lib
	@bash build-deb.sh utils
	@bash build-deb.sh navi
	@bash build-deb.sh doc

tar-balls:
	@tar cvzf $(BEAGLE)_$(BEAGLE_RELEASE).tar.gz $(BEAGLE) --exclude=.svn > /dev/null
	@tar cvzf $(LIB)_$(LIB_RELEASE).tar.gz $(LIB) --exclude=.svn > /dev/null
	@tar cvzf $(UTILS)_$(UTILS_RELEASE).tar.gz $(UTILS) --exclude=.svn > /dev/null
	@tar cvzf $(NAVI)_$(NAVI_RELEASE).tar.gz $(NAVI) --exclude=.svn > /dev/null
	@tar cvzf $(DOC)_$(DOC_RELEASE).tar.gz $(DOC) --exclude=.svn > /dev/null
zips:
	@sh check.sh --cmd -e zip
	@zip -q -r $(BEAGLE)_$(BEAGLE_RELEASE).zip $(BEAGLE) -x *.svn*
	@zip -q -r $(LIB)_$(LIB_RELEASE).zip $(LIB) -x *.svn*
	@zip -q -r $(UTILS)_$(UTILS_RELEASE).zip $(UTILS) -x *.svn*
	@zip -q -r $(NAVI)_$(NAVI_RELEASE).zip $(NAVI) -x *.svn*
	@zip -q -r $(DOC)_$(DOC_RELEASE).zip $(DOC) -x *.svn*

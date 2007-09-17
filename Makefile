#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE=opendicom-beagle
LIB=opendicom-sharp
DOC=opendicom-sharp-doc
GDK=opendicom-sharp-gdk
UTILS=opendicom-utils
NAVI=opendicom-navigator

BEAGLE_RELEASE=$(shell cat $(BEAGLE)/release)
LIB_RELEASE=$(shell cat $(LIB)/release)
DOC_RELEASE=$(shell cat $(DOC)/release)
GDK_RELEASE=$(shell cat $(GDK)/release)
UTILS_RELEASE=$(shell cat $(UTILS)/release)
NAVI_RELEASE=$(shell cat $(NAVI)/release)


all: build

clean: clean-packages
	@make -C $(BEAGLE) clean
	@make -C $(LIB) clean
	@make -C $(GDK) clean
	@make -C $(UTILS) clean
	@make -C $(NAVI) clean

build:
	@make -C $(LIB) build
	@make -C $(GDK) build
	@make -C $(UTILS) build
	@make -C $(NAVI) build
	@make -C $(BEAGLE) build

install:
	@make -C $(LIB) install
	@make -C $(GDK) install
	@make -C $(UTILS) install
	@make -C $(NAVI) install
	@make -C $(BEAGLE) install

uninstall:
	@make -C $(BEAGLE) uninstall
	@make -C $(NAVI) uninstall
	@make -C $(UTILS) uninstall
	@make -C $(GDK) uninstall
	@make -C $(LIB) uninstall

clean-packages:
	@rm -f *.tar.gz *.zip *.deb

debs:
	@bash build-deb.sh beagle
	@bash build-deb.sh lib
	@bash build-deb.sh gdk
	@bash build-deb.sh utils
	@bash build-deb.sh navi
	@bash build-deb.sh doc

tar-balls:
	@tar cvzf $(BEAGLE)_$(BEAGLE_RELEASE).tar.gz $(BEAGLE) --exclude=.svn --exclude=*.pidb > /dev/null
	@tar cvzf $(LIB)_$(LIB_RELEASE).tar.gz $(LIB) --exclude=.svn --exclude=*.pidb > /dev/null
	@tar cvzf $(GDK)_$(GDK_RELEASE).tar.gz $(GDK) --exclude=.svn --exclude=*.pidb > /dev/null
	@tar cvzf $(UTILS)_$(UTILS_RELEASE).tar.gz $(UTILS) --exclude=.svn --exclude=*.pidb > /dev/null
	@tar cvzf $(NAVI)_$(NAVI_RELEASE).tar.gz $(NAVI) --exclude=.svn --exclude=*.pidb > /dev/null
	@tar cvzf $(DOC)_$(DOC_RELEASE).tar.gz $(DOC) --exclude=.svn --exclude=*.pidb > /dev/null
zips:
	@bash check.sh --cmd -e zip
	@zip -q -r $(BEAGLE)_$(BEAGLE_RELEASE).zip $(BEAGLE) -x *.svn* -x *.pidb
	@zip -q -r $(LIB)_$(LIB_RELEASE).zip $(LIB) -x *.svn* -x *.pidb
	@zip -q -r $(GDK)_$(GDK_RELEASE).zip $(GDK) -x *.svn* -x *.pidb
	@zip -q -r $(UTILS)_$(UTILS_RELEASE).zip $(UTILS) -x *.svn* -x *.pidb
	@zip -q -r $(NAVI)_$(NAVI_RELEASE).zip $(NAVI) -x *.svn* -x *.pidb
	@zip -q -r $(DOC)_$(DOC_RELEASE).zip $(DOC) -x *.svn* -x *.pidb

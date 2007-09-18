#!/usr/bin/make
# written by Albert Gnandt (http://www.gnandt.com/)
# $Id$

BEAGLE=opendicom-beagle
LIB=opendicom-sharp
DOC=opendicom-sharp-doc
UTILS=opendicom-utils
NAVI=opendicom-navigator

TARGETS=beagle lib doc utils navi


all: build

clean: clean-packages
	@make -C $(BEAGLE) clean
	@make -C $(LIB) clean
	@make -C $(UTILS) clean
	@make -C $(NAVI) clean

build:
	@make -C $(LIB) build
	@make -C $(UTILS) build
	@make -C $(NAVI) build
	@make -C $(BEAGLE) build

install:
	@make -C $(LIB) install
	@make -C $(UTILS) install
	@make -C $(NAVI) install
	@make -C $(BEAGLE) install

uninstall:
	@make -C $(BEAGLE) uninstall
	@make -C $(NAVI) uninstall
	@make -C $(UTILS) uninstall
	@make -C $(LIB) uninstall

clean-packages:
	@rm -f *.tar.gz *.zip *.deb

debs:
	@for PROJECT in $(TARGETS); do bash build-deb.sh $$PROJECT; done

tgzs: tarballs

tarballs:
	@for PROJECT in $(TARGETS); do bash bundle-project.sh --tgz $$PROJECT; done

zips:
	@bash check.sh --cmd -e zip
	@for PROJECT in $(TARGETS); do bash bundle-project.sh --zip $$PROJECT; done

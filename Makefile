#!/usr/bin/make
# written by Albert Gnandt (albert.gnandt@hs-heilbronn.de, http://www.gnandt.com/)


LIB=opendicom-sharp
UTILS=opendicom-utils
NAVI=opendicom-navigator
DOC=opendicom-sharp-doc


DICOM_SHARP_RELEASE=$(shell cat $(LIB)/release)
DICOM_UTILS_RELEASE=$(shell cat $(UTILS)/release)
DICOM_NAVI_RELEASE=$(shell cat $(NAVI)/release)


all: build

clean: clean-packages
	@make -C $(LIB) clean
	@make -C $(UTILS) clean
	@make -C $(NAVI) clean

build:
	@make -C $(LIB) build
	@make -C $(UTILS) build
	@make -C $(NAVI) build

install:
	@make -C $(LIB) install
	@make -C $(UTILS) install
	@make -C $(NAVI) install

uninstall:
	@make -C $(LIB) uninstall
	@make -C $(UTILS) uninstall
	@make -C $(NAVI) uninstall

clean-packages:
	@rm -f *.tar.gz *.zip *.deb

md5sum:
	@sh checksum.sh lib $(DICOM_SHARP_RELEASE)
	@sh checksum.sh utils $(DICOM_UTILS_RELEASE)
	@sh checksum.sh navi $(DICOM_NAVI_RELEASE)

debs: md5sum
	@dpkg -b $(LIB)_$(DICOM_SHARP_RELEASE)_deb $(LIB)_$(DICOM_SHARP_RELEASE)_all.deb
	@dpkg -b $(UTILS)_$(DICOM_UTILS_RELEASE)_deb $(UTILS)_$(DICOM_UTILS_RELEASE)_all.deb
	@dpkg -b $(NAVI)_$(DICOM_NAVI_RELEASE)_deb $(NAVI)_$(DICOM_NAVI_RELEASE)_all.deb	

tar-balls:
	@sh check.sh --dir -e $(LIB)_$(DICOM_SHARP_RELEASE) $(UTILS)_$(DICOM_UTILS_RELEASE) $(NAVI)_$(DICOM_NAVI_RELEASE) $(DOC)_$(DICOM_SHARP_RELEASE)
	@tar cvzf $(LIB)_$(DICOM_SHARP_RELEASE).tar.gz $(LIB)_$(DICOM_SHARP_RELEASE) > /dev/null
	@tar cvzf $(UTILS)_$(DICOM_UTILS_RELEASE).tar.gz $(UTILS)_$(DICOM_UTILS_RELEASE) > /dev/null
	@tar cvzf $(NAVI)_$(DICOM_NAVI_RELEASE).tar.gz $(NAVI)_$(DICOM_NAVI_RELEASE) > /dev/null
	@tar cvzf $(DOC)_$(DICOM_SHARP_RELEASE).tar.gz $(DOC)_$(DICOM_SHARP_RELEASE) > /dev/null
zips:
	@sh check.sh --cmd -e zip
	@sh check.sh --dir -e $(LIB)_$(DICOM_SHARP_RELEASE) $(UTILS)_$(DICOM_UTILS_RELEASE) $(NAVI)_$(DICOM_NAVI_RELEASE) $(DOC)_$(DICOM_SHARP_RELEASE)
	@zip -q -r $(LIB)_$(DICOM_SHARP_RELEASE).zip $(LIB)_$(DICOM_SHARP_RELEASE)
	@zip -q -r $(UTILS)_$(DICOM_UTILS_RELEASE).zip $(UTILS)_$(DICOM_UTILS_RELEASE)
	@zip -q -r $(NAVI)_$(DICOM_NAVI_RELEASE).zip $(NAVI)_$(DICOM_NAVI_RELEASE)
	@zip -q -r $(DOC)_$(DICOM_SHARP_RELEASE).zip $(DOC)_$(DICOM_SHARP_RELEASE)

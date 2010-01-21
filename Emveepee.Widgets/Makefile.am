BUILDDIR= $(top_builddir)/lib
ASSEMBLY = $(BUILDDIR)/Emveepee.Widgets.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
TARGETS = $(ASSEMBLY) $(ASSEMBLY_MDB)

CSFLAGS =  -codepage:utf8 -debug -define:DEBUG

pkglib_DATA = $(TARGETS)

CLEANFILES = $(TARGETS)

FILES =  \
	AllocationsView.cs \
	AssemblyInfo.cs \
	DisplayOptions.cs \
	History.cs \
	Node.cs \
	ProfileConfiguration.cs \
	ProfileOptionsEditor.cs \
	ProfileSetupDialog.cs \
	ProfileStore.cs \
	ProfileView.cs \
	ProfilerProcess.cs \
	ProfilerSocket.cs \
	StartPage.cs \
	StackView.cs

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES = \
	Monodevelop-logo.png

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))
build_resources_args = $(addprefix -resource:, $(build_resources))

REFERENCES =  \
	-pkg:gtk-sharp-2.0 \
	-r:Mono.Cairo \
	-r:Mono.Posix

PROJECT_REFERENCES = $(BUILDDIR)/Emveepee.Decoding.dll

build_references = $(addprefix /r:, $(PROJECT_REFERENCES)) $(REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources)
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = $(FILES) $(RESOURCES)


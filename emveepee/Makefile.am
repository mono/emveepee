BUILDDIR = $(top_builddir)/lib
ASSEMBLY = $(BUILDDIR)/emveepee.exe
ASSEMBLY_MDB = $(ASSEMBLY).mdb
TARGETS = $(ASSEMBLY) $(ASSEMBLY_MDB)

CSFLAGS =  -codepage:utf8 -debug "-define:DEBUG"

pkglib_DATA = $(TARGETS)
bin_SCRIPTS = emveepee

CLEANFILES = $(TARGETS)

FILES =  \
	AssemblyInfo.cs \
	Main.cs \
	MainWindow.cs 

build_sources = $(addprefix $(srcdir)/, $(FILES))

RESOURCES =

build_resources = $(addprefix $(srcdir)/, $(RESOURCES))
build_resources_args = $(addprefix -resource:, $(build_resources))

REFERENCES =  \
	$(GTKSHARP_LIBS) \
	-r:Mono.Posix

DLL_REFERENCES = 

PROJECT_REFERENCES =  \
	$(BUILDDIR)/Emveepee.Widgets.dll

build_references = $(addprefix -r:, $(PROJECT_REFERENCES)) $(REFERENCES)

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources) $(build_resources) $(PROJECT_REFERENCES)
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:exe $(build_sources) $(build_resources_args) $(build_references)

EXTRA_DIST = $(FILES) $(RESOURCES)


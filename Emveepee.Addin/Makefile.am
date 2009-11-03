BUILDDIR = $(top_builddir)/lib
ASSEMBLY = $(BUILDDIR)/Emveepee.Addin.dll

if ENABLE_ADDIN
TARGETS = $(ASSEMBLY) $(ASSEMBLY).mdb
ADDIN_FILE = Emveepee.addins
else
TARGETS =
ADDIN_FILE =
endif

addindir = $(MDADDINS_PATH)
addin_DATA = $(ADDIN_FILE)
pkglib_DATA = $(TARGETS)
CLEANFILES = $(TARGETS)

dependencies = \
	$(BUILDDIR)/Emveepee.Widgets.dll

references =  \
	$(GTKSHARP_LIBS) \
	$(ADDINS_LIBS) \
	-r:Mono.Posix

build_references = $(addprefix -r:, $(dependencies)) $(references)

manifest = Emveepee.Addin.addin.xml

FILES =  \
	AssemblyInfo.cs \
	ProfilerCommands.cs \
	ProfilerExecutionHandler.cs \
	ProfilerExecutionModeSet.cs \
	ProfilerLogDisplayBinding.cs \
	ProfilerNodeBuilderExtension.cs \
	ProfilerViewContent.cs

build_sources = $(addprefix $(srcdir)/, $(FILES))

$(ASSEMBLY): $(build_sources) $(dependencies) $(srcdir)/$(manifest)
	mkdir -p $(BUILDDIR)
	$(GMCS) -debug -out:$@ -target:library $(build_sources) $(build_references) -resource:$(srcdir)/$(manifest)

EXTRA_DIST = $(FILES) Emveepee.addins.in $(manifest)

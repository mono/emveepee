BUILDDIR= $(top_builddir)/lib
ASSEMBLY = $(BUILDDIR)/Emveepee.Decoding.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
TARGETS = $(ASSEMBLY) $(ASSEMBLY_MDB)

pkglib_DATA = $(TARGETS)
CLEANFILES = $(TARGETS)

FILES =  \
	AssemblyInfo.cs \
	Block.cs \
	DirectivesBlock.cs \
	EndBlock.cs \
	EventsBlock.cs \
	FilteredCallStack.cs \
	HeapDataBlock.cs \
	HeapSummaryBlock.cs \
	IntroBlock.cs \
	LoadedBlock.cs \
	LogFileStream.cs \
	MappingBlock.cs \
	ProfileData.cs \
	StackItem.cs \
	StackNode.cs \
	StatBlock.cs \
	UnloadedBlock.cs

CSFLAGS =  -codepage:utf8 -debug -define:DEBUG

build_sources = $(addprefix $(srcdir)/, $(FILES))

REFERENCES =

$(ASSEMBLY) $(ASSEMBLY_MDB): $(build_sources)
	mkdir -p $(BUILDDIR)
	$(GMCS) $(CSFLAGS) -out:$(ASSEMBLY) -target:library $(build_sources) $(references)

EXTRA_DIST = $(FILES)


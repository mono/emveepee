aclocal
automake -a --foreign
autoconf
./configure --enable-maintainer-mode $*

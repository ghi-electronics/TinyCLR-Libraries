# Build librlp.a — the static library users link against to call RLP_* helpers
# from their RLP-loaded C/C++ code.
#
# Use the same toolchain as the user's RLP build:
#     make CROSS_COMPILE=arm-none-eabi-

CROSS_COMPILE ?= arm-none-eabi-
CC      := $(CROSS_COMPILE)gcc
AS      := $(CROSS_COMPILE)gcc
AR      := $(CROSS_COMPILE)ar

ARCH_FLAGS := -mcpu=cortex-m7 -mthumb -mfloat-abi=hard -mfpu=fpv5-d16
PIC_FLAGS  := -mropi -mrwpi -mlong-calls
WARN_FLAGS := -Wall -Wextra

ASFLAGS := $(ARCH_FLAGS) $(PIC_FLAGS)
CFLAGS  := $(ARCH_FLAGS) $(PIC_FLAGS) $(WARN_FLAGS) -Os -ffunction-sections -fdata-sections -Iinclude

OBJS := src/rlp_stubs.o

lib/librlp.a: $(OBJS)
	@mkdir -p lib
	$(AR) rcs $@ $^

src/%.o: src/%.S
	$(AS) $(ASFLAGS) -c $< -o $@

clean:
	rm -f $(OBJS) lib/librlp.a

.PHONY: clean

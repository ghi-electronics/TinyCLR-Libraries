/*
 * RLP.h - User header for TinyCLR Runtime Loadable Procedures.
 *
 * Include this in your C/C++ RLP source. Link against librlp.a from the
 * TinyCLR SDK. Build with -mropi -mrwpi -mlong-calls so the produced ELF
 * is position independent — the runtime loads it into a heap-allocated
 * region at a non-fixed address.
 *
 *   arm-none-eabi-gcc -mcpu=cortex-m7 -mthumb -mfloat-abi=hard \
 *       -mfpu=fpv5-d16 -mropi -mrwpi -mlong-calls \
 *       -ffunction-sections -fdata-sections \
 *       -nostartfiles -nostdlib \
 *       -I <sdk>/rlp/include -L <sdk>/rlp/lib \
 *       my_code.c -lrlp -o my_code.elf
 *
 * RLP code runs UNPRIVILEGED. Hardware-enforced access:
 *   ALLOWED:  GPIO peripheral registers, your own .text/.data/.bss/heap/stack
 *   BLOCKED:  internal flash, FLASH controller, system registers, DMA,
 *             SPI, I2C, UART, timers, SDMMC, USB, ETH — the MPU faults.
 *
 * Use the RLP_* helpers below for anything outside that allow list. They
 * are SVC-trap stubs; the kernel handles them at privileged level after
 * argument validation.
 *
 * Copyright GHI Electronics, LLC 2026. Do not modify this header.
 */

#ifndef _TINYCLR_RLP_H_
#define _TINYCLR_RLP_H_

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

/* Mark each entry point you intend to find via ElfImage.FindFunction with
 * RLP_EXPORT. Without it, the linker's --gc-sections may drop functions
 * not reachable from the linker script's ENTRY symbol, and FindFunction
 * returns 0.
 *
 * Example:
 *     RLP_EXPORT int MyFunction(void** args) { ... }
 */
#define RLP_EXPORT __attribute__((used))

/* ---------------------------------------------------------------------------
 * GPIO
 * Pin numbering: port_index * 16 + pin_index.
 *   PA0 = RLP_PIN_PORT_A(0) = 0
 *   PB7 = RLP_PIN_PORT_B(7) = 23
 *   PK15 = RLP_PIN_PORT_K(15) = 175
 * --------------------------------------------------------------------------*/

#define RLP_PIN_PORT_A(p)  ( 0u * 16u + (p))
#define RLP_PIN_PORT_B(p)  ( 1u * 16u + (p))
#define RLP_PIN_PORT_C(p)  ( 2u * 16u + (p))
#define RLP_PIN_PORT_D(p)  ( 3u * 16u + (p))
#define RLP_PIN_PORT_E(p)  ( 4u * 16u + (p))
#define RLP_PIN_PORT_F(p)  ( 5u * 16u + (p))
#define RLP_PIN_PORT_G(p)  ( 6u * 16u + (p))
#define RLP_PIN_PORT_H(p)  ( 7u * 16u + (p))
#define RLP_PIN_PORT_I(p)  ( 8u * 16u + (p))
#define RLP_PIN_PORT_J(p)  ( 9u * 16u + (p))
#define RLP_PIN_PORT_K(p)  (10u * 16u + (p))

#define RLP_PIN_NONE       0xFFFFFFFFu

typedef enum {
    RLP_GPIO_EDGE_NONE    = 0,
    RLP_GPIO_EDGE_RISING  = 1,
    RLP_GPIO_EDGE_FALLING = 2,
    RLP_GPIO_EDGE_BOTH    = 3,
    RLP_GPIO_LEVEL_HIGH   = 4,
    RLP_GPIO_LEVEL_LOW    = 5
} RLP_GpioEdge;

typedef enum {
    RLP_GPIO_RESISTOR_NONE     = 0,
    RLP_GPIO_RESISTOR_PULLDOWN = 1,
    RLP_GPIO_RESISTOR_PULLUP   = 2
} RLP_GpioResistor;

/* GPIO interrupt callback. Runs in unprivileged thread mode (DPC) — NOT in
 * handler mode. Safe to call other RLP_* helpers from here. */
typedef void (*RLP_GpioIsr)(uint32_t pin, uint32_t state, void* param);

/* Returns: 0 on success, non-zero on failure (pin reserved by firmware,
 * invalid pin number, etc.). */
extern uint32_t RLP_Gpio_EnableOutput(uint32_t pin, uint32_t initialState);
extern uint32_t RLP_Gpio_EnableInput(uint32_t pin, RLP_GpioResistor resistor);
extern uint32_t RLP_Gpio_EnableInterruptInput(uint32_t pin,
                                              RLP_GpioEdge edge,
                                              RLP_GpioResistor resistor,
                                              RLP_GpioIsr isr,
                                              void* param);
extern uint32_t RLP_Gpio_Read(uint32_t pin);
extern void     RLP_Gpio_Write(uint32_t pin, uint32_t state);
extern uint32_t RLP_Gpio_Release(uint32_t pin);


/* ---------------------------------------------------------------------------
 * Time
 * --------------------------------------------------------------------------*/

extern void     RLP_Time_DelayMicroseconds(uint32_t microseconds);
extern uint64_t RLP_Time_GetTicks(void);          /* hi-res monotonic counter */
extern uint32_t RLP_Time_GetTicksPerSecond(void); /* tick frequency */


/* ---------------------------------------------------------------------------
 * Memory (allocates inside the RLP image's RW region, not the managed heap)
 * --------------------------------------------------------------------------*/

extern void* RLP_Memory_Allocate(uint32_t size);
extern void  RLP_Memory_Free(void* ptr);


/* ---------------------------------------------------------------------------
 * Task scheduler — deferred work and one-shot/repeating timers.
 * Callbacks run in unprivileged thread mode, never in handler mode.
 * --------------------------------------------------------------------------*/

typedef void (*RLP_TaskCallback)(void* arg);

#define RLP_TASK_DATA_SIZE 64
typedef struct {
    uint32_t _opaque[RLP_TASK_DATA_SIZE / 4];
} RLP_Task;

extern void     RLP_Task_Initialize(RLP_Task* task, RLP_TaskCallback callback, void* arg);
extern void     RLP_Task_Schedule(RLP_Task* task);                              /* fire ASAP */
extern void     RLP_Task_ScheduleAfter(RLP_Task* task, uint32_t microseconds);  /* fire after delay */
extern void     RLP_Task_Abort(RLP_Task* task);
extern uint32_t RLP_Task_IsScheduled(RLP_Task* task);


/* ---------------------------------------------------------------------------
 * Native -> managed event channel.
 * Calls the C# RuntimeLoadableProcedures.NativeEvent handler with `data`.
 * Asynchronous; returns to caller before the C# handler runs.
 * --------------------------------------------------------------------------*/

extern void RLP_PostManagedEvent(uint32_t data);

#ifdef __cplusplus
}
#endif

#endif /* _TINYCLR_RLP_H_ */

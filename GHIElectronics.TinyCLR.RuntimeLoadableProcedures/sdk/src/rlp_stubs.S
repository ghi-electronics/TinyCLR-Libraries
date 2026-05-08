/*
 * rlp_stubs.S — SVC trap stubs for the RLP_* helper functions.
 *
 * Each RLP_* helper is a one-instruction SVC trap. The kernel's SVC
 * handler reads the immediate from the trapping instruction, dispatches
 * to the right privileged service, validates arguments, and returns.
 *
 * SVC numbering scheme (must match the osRtxUserSVC[] table in
 * TinyCLR-Devices/Drivers/RuntimeLoadableProcedures/RlpSvc.cpp):
 *
 *      Range       Group
 *      0x00        RESERVED by RTX5 - DO NOT USE
 *      0x01..0x0F  GPIO
 *      0x10..0x1F  Time
 *      0x20..0x2F  Memory
 *      0x30..0x3F  Task
 *      0x40..0x4F  Event / misc
 *
 * SVC #0 must NEVER be assigned to a user helper - the SVC_Handler in
 * STM32H7_Startup.gcc.s routes SVC #0 to RTX5's internal kernel-service
 * path which reads the user's R12 register and BLXes to it. With our
 * stubs that path lands on garbage and faults.
 *
 * Build flags must match the user app: -mcpu=cortex-m7 -mthumb
 * -mfloat-abi=hard -mfpu=fpv5-d16 -fropi -frwpi -mlong-calls.
 *
 * Copyright GHI Electronics, LLC 2026.
 */

    .syntax unified
    .thumb

    .macro RLP_STUB name, svcid
    .global \name
    .type   \name, %function
    .thumb_func
\name:
    svc     #\svcid
    bx      lr
    .size   \name, . - \name
    .endm

/* ---- GPIO (0x01..0x06)  -- 0x00 is RTX-reserved -------------------------- */
    RLP_STUB RLP_Gpio_EnableOutput,         0x01
    RLP_STUB RLP_Gpio_EnableInput,          0x02
    RLP_STUB RLP_Gpio_EnableInterruptInput, 0x03
    RLP_STUB RLP_Gpio_Read,                 0x04
    RLP_STUB RLP_Gpio_Write,                0x05
    RLP_STUB RLP_Gpio_Release,              0x06

/* ---- Time (0x10..0x1F) -------------------------------------------------- */
    RLP_STUB RLP_Time_DelayMicroseconds,    0x10
    RLP_STUB RLP_Time_GetTicks,             0x11
    RLP_STUB RLP_Time_GetTicksPerSecond,    0x12

/* ---- Memory (0x20..0x2F) ------------------------------------------------ */
    RLP_STUB RLP_Memory_Allocate,           0x20
    RLP_STUB RLP_Memory_Free,               0x21

/* ---- Task scheduler (0x30..0x3F) --------------------------------------- */
    RLP_STUB RLP_Task_Initialize,           0x30
    RLP_STUB RLP_Task_Schedule,             0x31
    RLP_STUB RLP_Task_ScheduleAfter,        0x32
    RLP_STUB RLP_Task_Abort,                0x33
    RLP_STUB RLP_Task_IsScheduled,          0x34

/* ---- Event / misc (0x40..0x4F) ----------------------------------------- */
    RLP_STUB RLP_PostManagedEvent,          0x40

    .end

// Copyright (c) 2024 GHI Electronics LLC
// Based on OpENer library: Copyright (c) 2009, Rockwell Automation, Inc. ALL RIGHTS RESERVED.
// EtherNet/IP is a trademark of ODVA, Inc.

// This file is intentionally empty after Phase 3 cleanup.
//
// The CipServiceFunctionCode enum that lived here was a byte-for-byte duplicate of
// CIPServiceCode (see CIPServiceCode.cs). Both were just CIP service numbers — there
// was no semantic difference between "which service number to record" and "which
// handler to bind"; same values, different name. The InsertService method now takes
// CIPServiceCode for both the serviceCode and handlerCode parameters.
//
// File kept (instead of deleted) so existing csproj references still resolve until
// a follow-up commit removes the <Compile Include="CipServiceFunction.cs" /> line.

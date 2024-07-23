# Aptos-Csharp-SDK

All functionalities are identical to those provided in https://github.com/aptos-labs/Aptos-Unity-SDK `v1.1.0` but this version has been modified for .NET Core.
## Changes

- Added exception handling for RestClient calls.
- Modified Unity Coroutines to use async.

- Changed the Aptos mint event from `0x4::collection::MintEvent` to `0x4::collection::Mint`
- Fixed the Data JSON parsing error in TransactionEvent
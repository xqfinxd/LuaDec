# LuaDec - Lua Decompiler

A decompiler for Lua.

Rewritten `unluac` using `.Net` (developed by `tehtmi` using java).

Here is an example usage of LuaDec:

```
LuaDec src.luac -o result.lua
```

## Development Environment

- Visual Studio 2019
- .NET 4.5

## Unit Test

| lua version       |  result  |
| :---------------- | :------: |
| 5.0.3             | allpass  |
| 5.1.5             | allpass  |
| 5.2.4             | allpass  |
| 5.3.6             | allpass  |
| 5.4.6             | failed 4 |
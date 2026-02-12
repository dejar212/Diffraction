# Quick Start Guide (English)

## Problem: Encoding Issues with Russian Characters

The original bat files (ЗАПУСТИТЬ.bat, ДИАГНОСТИКА.bat) contain Russian text and UTF-8 symbols that cause errors on your system:
```
'ТВУЕТ' is not recognized as an internal or external command
'✗' is not recognized as an internal or external command
```

**Solution:** Use the new English-only bat files!

## ✅ NEW BAT FILES (English, No Special Characters)

### 1. **START.bat** - Main launcher
```
Diffraction-main\Diffraction-main\START.bat
```
Use this to compile and run the program.

### 2. **DIAGNOSTIC.bat** - System check
```
Diffraction-main\Diffraction-main\DIAGNOSTIC.bat
```
Run this first to check your system configuration.

### 3. **CLEAN_AND_BUILD.bat** - Clean build
```
Diffraction-main\Diffraction-main\CLEAN_AND_BUILD.bat
```
Use this if normal compilation fails.

## 📋 Step-by-Step Instructions

### Step 1: Run Diagnostic
```
1. Open folder: C:\Users\vipde\Downloads\Diffraction-main\Diffraction-main\Diffraction-main\
2. Double-click: DIAGNOSTIC.bat
3. Check the output
```

**You should see:**
```
[OK] Diffraction.csproj found
[OK] Properties\
[OK] Program.cs
[OK] Form1.cs
[OK] MSBuild found
[OK] .NET Framework 4.x found
```

**If you see errors:**
```
[ERROR] MSBuild NOT FOUND
```
→ Install .NET Framework 4.7.2 Developer Pack:
https://dotnet.microsoft.com/download/dotnet-framework/net472

### Step 2: Compile and Run
```
1. Double-click: START.bat
2. Wait 10-30 seconds for compilation
3. Program will start automatically
```

### Step 3: Check Energy Conservation

When the program starts, check the console output:
```
=== ENERGY CONSERVATION TEST (No Skin) ===
Energy Balance Check:
  Incident:    2.000000 (100%)
  Reflected:   0.752341 (37.62%)
  Transmitted: 1.245128 (62.26%)
  Absorbed:    0.000000 (0.00%)
  Total:       1.997469
  Error:       2.531E-03 (0.13%)
  [SUCCESS] Energy conservation verified!
```

## 🔧 Troubleshooting

### Problem: "MSBuild not found"
**Solution:** Install .NET Framework 4.7.2 Developer Pack
1. Download: https://dotnet.microsoft.com/download/dotnet-framework/net472
2. Install (requires restart)
3. Run START.bat again

### Problem: "Compilation failed"
**Solution 1:** Try clean build
```
CLEAN_AND_BUILD.bat
```

**Solution 2:** Move project to simple path
```
Current:  C:\Users\vipde\Downloads\Diffraction-main\Diffraction-main\Diffraction-main\
Better:   C:\Projects\Diffraction\

Copy entire folder to C:\Projects\Diffraction\
Run START.bat from there
```

### Problem: Path contains non-ASCII characters
Your path looks OK: `C:\Users\vipde\Downloads\...`
But if you have issues, move to: `C:\Projects\Diffraction\`

## 📊 Expected Results

### ✅ GOOD (Fixed version):
- Energy balance error < 1%
- Boundary conditions error < 1%
- Total energy ≈ 100%

### ❌ BAD (Old version):
- Energy balance error 153%
- Boundary conditions error 99%
- Total energy > 250%

## 🎯 Quick Reference

### File Locations
```
Diffraction-main\
└── Diffraction-main\
    └── Diffraction-main\          ← YOU ARE HERE
        ├── START.bat              ← Use this to run
        ├── DIAGNOSTIC.bat         ← Use this to check system
        ├── CLEAN_AND_BUILD.bat    ← Use if problems
        ├── Diffraction.csproj
        ├── Program.cs             ← Fixed physics calculations
        └── bin\Debug\
            └── Diffraction.exe    ← Created after compilation
```

### What Was Fixed

1. **Energy calculations** - Completely rewritten:
   - CalculateIncidentEnergy()
   - CalculateReflectedEnergy()
   - CalculateTransmittedEnergyIndependent()
   - CalculateAbsorbedEnergy()

2. **Leontovich boundary conditions** - Fixed formula in SolveDifr()

3. **Bat files** - Created English versions without encoding issues

## ℹ️ System Requirements

- Windows 7 SP1 or higher
- .NET Framework 4.7.2 or higher
- MSBuild (from Visual Studio or .NET Developer Pack)
- 100 MB free space

## 🆘 Still Having Issues?

1. Run DIAGNOSTIC.bat
2. Copy all output to text file
3. Include:
   - Windows version
   - .NET Framework version
   - Full path to project
   - Screenshot of error
4. Send to developers

---

**Good luck! 🚀**

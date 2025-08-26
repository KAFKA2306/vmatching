# VRChat World UI and Material Fixes

This document explains the VRChat-specific fixes implemented in the VTMSceneBuilder to address common issues in VRChat world development.

## Issues Fixed

### 1. UI Canvas Following Player (Screen Space Overlay Issue)

**Problem**: UI Canvas was set to `Screen Space - Overlay` mode, causing it to follow the player's view like a HUD.

**Solution**: 
- Changed Canvas `renderMode` to `RenderMode.WorldSpace`
- Positioned canvas on the north wall at coordinates (0, 2f, 9.5f) 
- Scaled down to 0.005 for appropriate world space sizing
- Added VRChat interaction components (BoxCollider, VRCUiShape)
- Set CanvasScaler to `ConstantPixelSize` mode

**Usage**:
```csharp
// Automatic fix during world creation - no action needed
// Or manually fix existing scenes:
VTMSceneBuilder.FixCanvasToWorldSpace();
```

### 2. Floor Material Color Changes

**Problem**: Floor materials using Standard shader with lighting effects caused colors to change when moving.

**Solution**:
- Created stable white Unlit material (`Mat_FloorWhite_Stable.mat`)
- Applied to all floor objects automatically
- Disabled shadow casting and receiving to prevent lighting changes
- Used `Unlit/Color` shader for consistent appearance

**Usage**:
```csharp
// Apply to all floors in scene
VTMSceneBuilder.FixFloorMaterials();

// Or access the material directly
Material whiteFloor = VTMSceneBuilder.WhiteFloorMaterial;
```

### 3. VRChat-Optimized Wall-Mounted UI

**Enhanced Features**:
- Wall-mounted positioning with proper scaling
- Enhanced button visibility with outlines
- Larger fonts for Quest compatibility  
- Emissive background for dark area visibility
- VRChat interaction colliders

## Menu Commands Added

### VTM Menu Commands:

1. **VTM/Apply All VRChat Fixes** - Applies all fixes at once
2. **VTM/Fix Canvas to World Space** - Converts UI to wall-mounted
3. **VTM/Fix Floor Materials to White** - Applies stable white floors
4. **VTM/Setup Materials** - Enhanced to include stable materials

## Implementation Details

### World Space Canvas Configuration
```csharp
// Position on north wall of lobby
canvas.transform.position = new Vector3(0, 2f, 9.5f);
canvas.transform.rotation = Quaternion.Euler(0, 180, 0); // Face inward
canvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

// VRChat interaction setup
BoxCollider collider = canvas.AddComponent<BoxCollider>();
collider.size = new Vector3(800, 600, 0.1f);
collider.isTrigger = true;
```

### Stable Floor Material
```csharp
Material whiteMat = new Material(Shader.Find("Unlit/Color"));
whiteMat.color = Color.white;

// Apply to renderer
renderer.sharedMaterial = whiteMat;
renderer.shadowCastingMode = ShadowCastingMode.Off;
renderer.receiveShadows = false;
```

### Button Enhancement for Wall Display
```csharp
// Larger fonts and outlines for Quest visibility
textComponent.fontSize = 24;
textComponent.fontStyle = TMPro.FontStyles.Bold;
textComponent.outlineWidth = 0.2f;
textComponent.outlineColor = Color.black;
```

## Performance Considerations

- Unlit shaders reduce GPU overhead
- Disabled shadows prevent unnecessary calculations
- Constant pixel size UI scales properly across devices
- Single material reduces draw calls

## VRChat Compliance

- World Space UI works with VRChat's laser pointers
- Quest-optimized with proper font sizes and contrast
- Performance-friendly materials and shaders
- Proper collider setup for interaction

## Testing Checklist

After applying fixes, verify:
- [ ] UI no longer follows player movement
- [ ] Floor color remains consistent when moving
- [ ] Buttons are clearly visible and clickable
- [ ] Text is readable from normal interaction distance
- [ ] Performance maintains 60+ FPS on Quest
- [ ] No VRChat build errors or warnings

## Notes

- VRC SDK components (VRCUiShape) are added conditionally
- Materials are saved to `Assets/VirtualTokyoMatching/Materials/`
- All fixes are compatible with headless build processes
- Original material setup is preserved for backward compatibility
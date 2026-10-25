# Art direction and generation prompts

All three new raster assets were created with the built-in OpenAI image-generation tool. They are project assets, not screenshots of an executed Unity build.

## environment

Use case: stylized-concept
Asset type: final production background panorama for a polished 2D side scrolling dinosaur runner in Unity, landscape 16:9, 1536x864 or larger.
Primary request: a completely original premium hand-painted fantasy game world, a moonlit bioluminescent jungle around ancient stone ruins. Deep midnight indigo sky, a large soft amber moon slightly right of center, sweeping teal mountains, floating dust and distant fireflies, carved monumental ruins wrapped in plants, turquoise light emerging from old stone seams, subtle magenta flowers.
Composition: strictly side-on 2D game environment, layered depth, not isometric. Horizon around the middle. Preserve a calm readable open band across the lower third for the runner and obstacles. The lowest fifth is dark near-ground fog. Left and right margins must have matching dark values and matching broad silhouettes to support horizontal repeating. No foreground objects that hide the playing band.
Style: beautifully painted stylized indie game, smooth graphic shapes, lush atmospheric depth and crisp contours, refined color control, no pixel art, no photorealism.
Constraints: no characters, no dinosaur, no obstacles, no coins, no interface, no text, no border, no watermarks. Actual game background asset, not a screenshot or a mockup.

## foliage

Use case: stylized-concept
Asset type: final transparent foreground/parallax strip for a premium hand painted 2D side scrolling game, wide 1536x864.
Primary request: one continuous horizontal strip of lush stylized jungle ferns, small mossy stones, tiny glowing cyan mushrooms and a few violet flowers along the bottom 20 percent. Deep navy-teal silhouettes with restrained luminous edges. Very small foliage height variation. Everything above the bottom 25 percent is truly transparent.
Composition: side-on flat 2D game view. No ground plane perspective, no horizon. Strip vegetation continues evenly from the left edge to the right edge, suitable for repeating scrolling. No trees, tall trunks, large leaves in the middle or detached objects.
Style: smooth premium hand-painted fantasy platformer asset, readable shapes, delicate cyan rim light, coordinated moonlit teal jungle palette, not pixel art.
Constraints: genuinely transparent background, no opaque backdrop, no gradient background, no fake checkerboard, no cast shadow outside the vegetation, no text or labels or watermark.

## atlas

Use case: stylized-concept
Asset type: final production transparent 4 by 4 sprite atlas for a premium 2D side view dinosaur runner.
Primary request: exactly 16 isolated game sprites arranged in a mathematically regular 4-column by 4-row grid on a square 2048 by 2048 transparent canvas. Each invisible grid cell is exactly 512 by 512 pixels. No gutters, no outer margin added to canvas, no visible grid lines.
Style: beautifully illustrated smooth clean 2D cartoon game sprites, crisp dark navy outline, controlled soft shading, luminous accents, premium indie fantasy game rather than pixel art.
Hero: the SAME small athletic friendly turquoise T-rex in every hero frame, warm cream throat and belly, tiny amber back spikes, amber eyes, small forearms, expressive head, long balancing tail. Entire body seen in strict side profile FACING RIGHT, no clothing or accessories. Strong readable silhouette. Keep head, torso and tail registration and body size consistent across frames. Each hero fits inside its cell with at least 55 transparent pixels margin; feet align to a baseline 64 pixels above each cell bottom.
EXACT CELL CONTENTS, read left to right, top to bottom:
Row 1: hero run animation pose A left foot forward; hero run pose B both legs passing; hero run pose C right foot forward; hero run pose D legs stretched with subtle airborne running bounce.
Row 2: hero jumping pose with tucked legs; hero hurt recoil pose with eyes squeezed shut but no blood; hero defeated lying on side (still whole dinosaur, non-graphic); hero fast dash pose body angled forward with tail straight, WITHOUT a speed trail attached.
Row 3: one tall violet crystal obstacle rooted in a small mossy rock; a low cluster of three cyan crystals rooted in a small mossy rock; one small hostile amber-and-purple flying beetle wings up FACING LEFT; the SAME beetle wings down FACING LEFT.
Row 4: one bright golden collectible coin front view with a simple engraved diamond; SAME coin turned one quarter; SAME coin in narrow side view; one compact circular cyan magic impact spark with amber center.
For obstacles feet/base also align at 64 pixels above the cell bottom. Beetles, coins and spark are centered within their cells. Every object is entirely contained in its own cell and separated from all other cells by transparent space.
Constraints: transparent alpha background, no opaque backdrop, no checkerboard, no text, no letters, no labels, no watermark, no frame numbers, no UI, no environment, no pedestal, no individual drop shadow. Sixteen sprites only in the specified exact order.

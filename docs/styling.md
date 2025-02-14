# Styling

Styling allows formatting text output with various styles such as bold, italic, underline, and colors. It enables visually distinct messages by applying styles dynamically within an Output operation.

---

## Using Styling in Output

Styling is applied using double brackets (`[[ ]]`). You can define a style at the beginning and reset it back to default using `[[/]]`.

### Example:
```json
{
    "operation": "Output",
    "value": "Command-Weaver by [[b]]Marek Starosielec[[/]]"
}
```

## Available Styles

| Code | Description |
|------|-------------|
| **b** / **bold** | Makes the text bold. |
| **i** / **italic** | Makes the text italic. |
| **u** / **underline** | Underlines the text. |

You can use either the short or full name for each style.


## Combining Multiple Styles

Multiple styles can be applied by separating them with a comma.

### Example:
```json
{
    "operation": "Output",
    "value": "[[b,i]]Bold and Italic[[/]]"
}
```
### Explanation:
- The Output operation applies both bold and italic styles to the text.
- The result is displayed as **_Bold and Italic_**.

## Using Colors

You can apply colors using **hex codes** in the `#RRGGBB` format.

### Example:
```json
{
    "operation": "Output",
    "value": "[[#FF5733]]Colored Text[[/]]"
}
```
You can also combine colors with styles:
```json
{
    "operation": "Output",
    "value": "[[#008000,b]]Bold Green Text[[/]]"
}
```

### Explanation:
- The color is specified using a hex code inside `[[ ]]`.
- In the first example, `#FF5733` applies an **orange** color to the text.
- In the second example, `#008000` applies a **green** color, and `b` makes the text bold.


## Using Styles from Variables

Styles can be stored in variables and referenced dynamically.

### Example:
```json
{
    "variables": [
        {
            "key": "highlightStyle",
            "value": "b,#FFD700"
        }
    ],
    "commands": [
        {
            "name": "styled-output",
            "operations": [
                {
                    "operation": "Output",
                    "value": "Important message: [[{{highlightStyle}}]]Warning![[/]]"
                }
            ]
        }
    ]
}
```
### Explanation:
- Variable `highlightStyle` is set to `"b,#FFD700"` (bold and gold color).
- Output operation applies this style dynamically.
- Text `"Warning!"` appears in **bold gold**.

## Resetting Styles

To return to the default text style, use `[[/]]`.

### Example:
```json
{
    "operation": "Output",
    "value": "Normal [[b]]Bold[[/]] Normal again."
}
```
### Explanation:
- `[[b]]` tag applies bold styling to the text.
- `[[/]]` tag resets the style back to default.
- Output will be: Normal **Bold** Normal again.*

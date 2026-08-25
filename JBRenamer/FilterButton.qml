import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts

Control {
    id: button
    required property string tooltip
    required property string source
    required property bool filterEnabled
    property int iconSize: 24
    padding: 5
    implicitHeight: Math.max(implicitBackgroundHeight + topInset + bottomInset,
        iconSize + topPadding + bottomPadding)
    implicitWidth: Math.max(implicitBackgroundWidth + leftInset + rightInset,
        iconSize + leftPadding + rightPadding)
    opacity: (filterEnabled ? 1.0 : 0.0)

    contentItem: Image {
        Layout.preferredWidth: button.iconSize
        Layout.preferredHeight: button.iconSize
        source: button.source

        ToolTip {
            visible: (hover.hovered && (button.opacity > 0))
            text: button.tooltip
        }

        MouseArea {
            id: ma
            anchors.fill: parent

            onClicked: filtersDialog.open()
        }

        HoverHandler {
            id: hover
        }
    }
}

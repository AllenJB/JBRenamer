import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs

ToolBar {
    Layout.fillWidth: true

    signal fileListUpdated()

    onFileListUpdated: function () {
        fileCount.text = files.count() + " items";
    }

    RowLayout {

        Label {
            text: "TODO Drop Filters"
        }
        ToolSeparator {}
        Label {
            id: fileCount
            text: files.count() + " items"
            
            horizontalAlignment: Qt.AlignLeft
            verticalAlignment: Qt.AlignVCenter
        }
    }
}

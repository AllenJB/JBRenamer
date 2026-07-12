import QtQuick
import QtQuick.Controls
import QtQuick.Layouts
import QtQuick.Dialogs;

ApplicationWindow {
    id: mainWindow
    visible: true
    title: "JBRenamer"
    width: 640 
    height: 480
    
    FilesModel {
        id: files
    }
    
    FileDialog {
        id: addSourceFileDialog
        acceptLabel: "Add Source File(s)"
        fileMode: FileDialog.OpenFiles
        options: FileDialog.DontResolveSymlinks | FileDialog.HideNameFilterDetails
        onAccepted: {
            files.AddSourceFile(selectedFile);
        }
    }

    menuBar: MenuBar {
        Menu {
            title: qsTr("&File")
            
            Action {
                text: "Add Source File(s)"
                onTriggered: addSourceFileDialog.open()
            }
            Action {
                text: qsTr("&Quit")
                onTriggered: mainWindow.close()
            }
        }
    }
    ColumnLayout {
        anchors.fill: parent;
        Rectangle {
            color: "darkseagreen"
            Layout.fillWidth: true
            Layout.fillHeight: true
        }
        TableView {
            id: fileTable
            model: files
            Layout.fillWidth: true
            Layout.fillHeight: true
            selectionBehavior: TableView.SelectRows
            selectionMode: TableView.ExtendedSelection
        }
    }
}

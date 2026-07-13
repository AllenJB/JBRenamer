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
            files.addSourceFile(selectedFile);
            Qt.callLater(function() {
                fileTable.forceLayout()
            })
        }
    }

    menuBar: MenuBar {
        Menu {
            title: qsTr("&File")
            
            Action {
                text: "Add Source File(s)"
                onTriggered: {
                    addSourceFileDialog.open()
                }
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
        HorizontalHeaderView {
            Layout.row: 1
            Layout.column: 1
            Layout.fillWidth: true
            syncView: fileTable
        }
        TableView {
            id: fileTable
            model: files
            Layout.fillWidth: true
            Layout.fillHeight: true
            alternatingRows: true
            columnSpacing: 2
            selectionBehavior: TableView.SelectRows
            selectionMode: TableView.ExtendedSelection
            editTriggers: TableView.NoEditTriggers
            delegate: TableViewDelegate {
                implicitHeight: 40
                implicitWidth: 9 * mainWindow.width / 20
                leftPadding: 10; topPadding: 10
            }
            ScrollBar.vertical: ScrollBar {
                policy: ScrollBar.AlwaysOn
            }
        }
    }
}

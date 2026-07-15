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
                    console.log("test")
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
        DropArea {
            Layout.fillWidth: true
            Layout.fillHeight: true
            
            onDropped: function(drop) {
                files.drop(drop.formats, drop.text, drop.urls)
                Qt.callLater(function() {
                    fileTable.forceLayout()
                })
            }

            ColumnLayout {
                anchors.fill: parent

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
                        implicitWidth: 9 * mainWindow.width / 20
                        padding: 5
                    }
                    ScrollBar.vertical: ScrollBar {
                        policy: ScrollBar.AlwaysOn
                    }
                }
            }
        }
    }
}

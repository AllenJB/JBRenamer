import QtQuick
import QtQuick.Controls.Fusion
import QtQuick.Layouts
import QtQuick.Dialogs
import QtCore
// The ../ is required here because that's how the files end up laid out in the bin/Debug/net10.0/ directory
import "../Files" as Files
import "../Filters" as Filters
import "../Presets" as Presets
import "../Rules" as Rules

ApplicationWindow {
    id: mainWindow
    visible: true
    title: "JBRenamer"
    width: 800
    height: 600

    Settings {
        location: "file://" + program.getConfigFile()
        category: "window"
        property alias x: mainWindow.x
        property alias y: mainWindow.y
        property alias width: mainWindow.width
        property alias height: mainWindow.height
    }

    Program {
        id: program

        signal fileListUpdated()

        signal ruleListUpdated()

        signal filtersUpdated()

        onShowError: function (args) {
            popupError.message = args.message;
            popupError.open();
        }

        onFileListUpdated: function () {
            program.files.runRules(program.rules);
        }

        onRuleListUpdated: function () {
            program.files.runRules(program.rules);
        }
    }

    onClosing: function () {
        program.onClosing();
    }

    DebugModel {
        id: debug
    }

    Dialog {
        id: popupError
        property string message
        title: "Error!"
        anchors.centerIn: parent
        modal: true
        focus: true
        closePolicy: Popup.CloseOnEscape
        standardButtons: Dialog.Ok
        Text {
            Layout.alignment: Qt.AlignCenter
            text: popupError.message
        }
    }

    FileDialog {
        id: addSourceFileDialog
        acceptLabel: "Add Source File(s)"
        fileMode: FileDialog.OpenFiles
        options: FileDialog.DontResolveSymlinks | FileDialog.HideNameFilterDetails
        onAccepted: {
            try {
                program.files.addSourceFile(selectedFile);
            } catch (e) {
                console.log("Xception:", e);
                Debug.log("Exception: " + e);
            }
            program.fileListUpdated();
        }
    }

    FolderDialog {
        id: addSourceDirDialog
        acceptLabel: "Add Source Files From Directory"
        options: FolderDialog.DontResolveSymlinks
        onAccepted: {
            program.files.addSourceDirectory(selectedFolder);
            program.fileListUpdated();
        }
    }

    Filters.Dialog {
        id: filtersDialog
    }

    Presets.AddDialog {
        id: presetAddDialog
    }

    menuBar: MenuBar {
        Menu {
            title: "App"

            Action {
                text: "&Quit"
                onTriggered: mainWindow.close()
            }
        }

        Files.Menu {
        }
        Rules.Menu {
        }
        Presets.Menu {
        }
    }

    footer: StatusBar {
        id: statusBar
    }

    Rules.AddDialog {
        id: ruleAddDialog
    }

    SplitView {
        id: listsContainer
        anchors.fill: parent
        orientation: Qt.Vertical

        Rules.RulesView {
            Layout.fillWidth: true
            SplitView.preferredHeight: (listsContainer.height / 2)
            SplitView.minimumHeight: 100
        }

        Files.FilesView {
            Layout.fillWidth: true
            SplitView.preferredHeight: (listsContainer.height / 2)
            SplitView.minimumHeight: 100
        }
    }
}

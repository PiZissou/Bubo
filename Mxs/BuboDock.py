'''
     Demonstrates how to create a QWidget with PySide2 and attach it to the 3dsmax main window.
     Creates two types of dockable widgets, a QDockWidget and a QToolbar
 '''
  
import os
import ctypes

from PySide2 import QtCore
from PySide2 import QtGui
from PySide2.QtWidgets import QMainWindow, QDockWidget, QToolButton, QToolBar, QAction, QLabel

from pymxs import runtime as rt
from qtmax import GetQMaxMainWindow


class PyDocking :  

     def __init__(self):
          self.mainWindow = GetQMaxMainWindow()
          self.dockW = None
          dockWidgets = [w for w in self.mainWindow.children() if w.objectName() == 'ActionDock']
          if (len (dockWidgets)>0):
               self.dockW = dockWidgets[0]

     def detect_ui(self):
          self.dockW = None
          dockWidgets = [w for w in self.mainWindow.children() if w.objectName() == 'ActionDock']
          if (len (dockWidgets)>0):
               self.dockW = dockWidgets[0]
                        
     def set_ui(self , uiName ):
          self.dockW = None
          dockWidgets = [w for w in self.mainWindow.children() if w.objectName() == uiName ]
          if (len (dockWidgets)>0):
               self.dockW = dockWidgets[0]

     def print_mainWindow_children(self):
          for w in self.mainWindow.children():
               print( w.objectName() )
               #print( w.windowTitle() )

     def debug_ui(self):
          if self.dockW != None:
               width  = self.dockW.width()
               height = self.dockW.height()
               self.dockW.resize( width + 1, height)
               self.dockW.resize( width - 1, height)
               

     def set_title(self, title):
          if self.dockW != None:
               self.dockW.setWindowTitle(title)
               #widget = QLabel(title, mainWindow) 
               #w.setTitleBarWidget(widget)

     def show(self):
          if self.dockW != None:
               self.dockW.show()

     def close(self):
          if self.dockW != None:
               self.dockW.close()

     def is_visible(self):
          res = self.dockW.isVisible()
          rt.PyDocking_is_visible =  res
          return res 
          
     def set_docking(self):
          if self.dockW != None:
               #self.mainWindow.addDockWidget(QtCore.Qt.RightDockWidgetArea, self.dockW)
               self.dockW.setFloating(False)

     def set_floating(self):
          if self.dockW != None:
               self.dockW.setFloating(True)

     def define_dock_area(self):
          if self.dockW != None:
               self.dockW.setAllowedAreas(QtCore.Qt.LeftDockWidgetArea | QtCore.Qt.RightDockWidgetArea )






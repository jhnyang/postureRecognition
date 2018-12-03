import sys
import pickle
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

from sklearn import svm, metrics
from sklearn.naive_bayes import GaussianNB

naive_G, knn, svm_clf, ran_forest, desisionT = pickle.load(open("models.pk1",'rb'))

x_predict = str(sys.argv[1]).split(' ')
x_predict = list(map(float, x_predict))
x_predict = [x_predict]

x = pd.DataFrame(x_predict)
x.iloc[:,::3] =x.iloc[:,0::3].sub(x.loc[:,0], axis=0)
x.iloc[:,1::3] =x.iloc[:,1::3].sub(x.loc[:,1], axis=0)
x.iloc[:,2::3] =x.iloc[:,2::3].sub(x.loc[:,2], axis=0)

y_predict = naive_G.predict(x)
print(y_predict) # numpy.ndarray